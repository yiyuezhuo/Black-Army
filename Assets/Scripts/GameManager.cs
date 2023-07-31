using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using UnityEngine.Events;
using System.Linq;

public class GameManager : MonoBehaviour
{
    GameState state;

    public UnityEvent stepped;
    public UnityEvent newTurnArrived;
    public UnityEvent<Detachment> detachmentSelected;
    public UnityEvent detachmentDeselected;
    public UnityEvent detachmentsChanged;
    public UnityEvent<Detachment> currentDetachmentMovingStateChanged;

    Hex lastSelectedHex;
    int currentSelectingIdx;

    Detachment currentDetachment;

    private void Awake()
    {
        state = Provider.state;
    }

    public void Step()
    {
        var turnBefore = state.Turn;
        state.NextPhase();
        OnStepped();
        stepped.Invoke();
        if (state.Turn != turnBefore)
        {
            lastSelectedHex = null;
            OnNewTurnArrived();
            newTurnArrived.Invoke();
        }

        Debug.Log(state);
    }

    void OnNewTurnArrived()
    {
        Provider.Message($"New Turn: {state.Turn}");
    }

    void OnStepped()
    {
        currentDetachment = null;
        detachmentDeselected.Invoke(); // TODO: refactor
        lastSelectedHex = null;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDetachmentsChanged() => detachmentsChanged.Invoke();

    public void stackClicked(Hex hex, Side side)
    {
        // Debug.Log($"stackClicked({hex}, {side})");

        if (side != state.CurrentSide) // Block non-current selection
        {
            Provider.Message($"Can't issue orders to {side.Name} units since the current side is {state.CurrentSide.Name}");
            return;
        }

        var currentSideStack = hex.Detachments.Where(d => d.Side == side).ToList();
        currentSelectingIdx = hex == lastSelectedHex ? (currentSelectingIdx + 1) % currentSideStack.Count : 0;
        lastSelectedHex = hex;
        var detachment = currentSideStack[currentSelectingIdx];

        Debug.Log($"currentSideStack.Count={currentSideStack.Count}, currentSelectingIdx={currentSelectingIdx}, hex == lastSelectedHex:{hex == lastSelectedHex}");

        OnDetachmentSelected(detachment);
        detachmentSelected.Invoke(detachment);
    }

    void OnDetachmentSelected(Detachment detachment)
    {
        currentDetachment = detachment;
    }

    public void OnHexRightClicked(Hex hex)
    {
        if (currentDetachment == null)
            return;

        if(currentDetachment.Hex == hex)
        {
            currentDetachment.MovingState = null;
            currentDetachmentMovingStateChanged.Invoke(currentDetachment);
            return;
        }

        var graph = new HexGraph() { detachment=currentDetachment};
        var path = YYZ.PathFinding.PathFinding<Hex>.AStar(graph, currentDetachment.Hex, hex);

        var s = string.Join(',', path.Select(hex => hex.ToString()));
        Debug.Log($"path=[{path.Count}]:{s}");

        if (path.Count < 2)
        {
            Provider.Message("Not reachable");
            return;
        }

        // Provider.Message($"Set Path:{path.Count}");
        Provider.Message("");

        var currentTarget = path[1];
        var waypoints = path.Skip(2).ToList();
        currentDetachment.MovingState = new MovingState() { CurrentCompleted = 0, CurrentTarget = currentTarget, Waypoints = waypoints };

        currentDetachmentMovingStateChanged.Invoke(currentDetachment);
    }

    public void OnUnitBarClosed()
    {
        detachmentDeselected.Invoke();
        currentDetachment = null;
    }
}

public class HexGraph: YYZ.PathFinding.IGraph<Hex>
{
    public Detachment detachment;

    public float MoveCost(Hex src, Hex dst) => detachment.Side.RailroadMovementAvailable && src.EdgeMap[dst].Railroad ? 1 : 10; // TODO: We should use different graph for graph unit type but for now we just use a simple graph due to time budget.
    public float EstimateCost(Hex src, Hex dst)
    {
        var dx = src.X - dst.X;
        var dy = src.Y - dst.Y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public IEnumerable<Hex> Neighbors(Hex src)
    {
        if(src.Type == "Ocean1") // TODO: refactor
        {
            yield break;
        }
        foreach((var hex, var edge) in src.EdgeMap)
        {
            if (edge.CountryBoundary)
                continue;
            if (hex.Type == "Ocean1") // TODO: refactor
                continue;
            yield return hex;
        }
    }
}