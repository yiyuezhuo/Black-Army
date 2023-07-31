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
    public UnityEvent detachmentsChanged;

    Hex lastSelectedHex;
    int currentSelectingIdx;

    private void Awake()
    {
        state = Provider.state;
    }

    public void Step()
    {
        var turnBefore = state.Turn;
        state.NextPhase();
        stepped.Invoke();
        if (state.Turn != turnBefore)
        {
            lastSelectedHex = null;
            newTurnArrived.Invoke();
        }

        Debug.Log(state);
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
        // TODO: Block non-current selection here?

        var currentSideStack = hex.Detachments.Where(d => d.Side == side).ToList();
        currentSelectingIdx = hex == lastSelectedHex ? (currentSelectingIdx + 1) % currentSideStack.Count : 0;
        lastSelectedHex = hex;
        var detachment = currentSideStack[currentSelectingIdx];
        Debug.Log($"currentSideStack.Count={currentSideStack.Count}, currentSelectingIdx={currentSelectingIdx}, hex == lastSelectedHex:{hex == lastSelectedHex}");
        detachmentSelected.Invoke(detachment);
    }
}
