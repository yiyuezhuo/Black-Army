using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using YYZ.BlackArmy.CombatResolution;
using YYZ.BlackArmy.Model;

public class CombatResolutionMergerTest : MonoBehaviour
{
    public int FixedItemHeight = 54;

    public VisualTreeAsset SubCombatLisyEntryTemplate;
    public VisualTreeAsset CombatResolutionTemplate;
    // public VisualTreeAsset StrengthStatsRowTemplate;
    public VisualTreeAsset TabsTemplate;

    UIDocument doc;

    List<CombatResolutionController.SnapshotData> dataList = new();

    // Start is called before the first frame update
    void Start()
    {
        doc = GetComponent<UIDocument>();

        var state = Provider.state;
        state.NextPhase();
        state.NextPhase();

        foreach(var group in dataList.GroupBy(data => data.LocationName))
        {
            var controller = new TabsController();
            var tabElement = TabsTemplate.Instantiate();
            controller.SetVisualElement(tabElement);

            var elements = new List<(string, VisualElement)>();
            var idx = 0;
            foreach (var data in group)
            {
                (var subTurnElement, var subTurnController) = CreateCombatResolution(data);
                subTurnController.ConfirmButton.RegisterCallback<ClickEvent>(evt => tabElement.RemoveFromHierarchy());

                elements.Add((idx.ToString(), subTurnElement));
                idx++;
            }

            controller.SetData(elements);
            tabElement.AddManipulator(new SimpleDraggingManipulator());
            tabElement.style.position = Position.Absolute;

            doc.rootVisualElement.Add(tabElement);
        }
    }

    private void OnEnable()
    {
        Provider.state.CombatResolved += OnCombatResolved;
    }

    private void OnDisable()
    {
        Provider.state.CombatResolved -= OnCombatResolved;
    }

    public void OnCombatResolved(object sender, GameState.CombatMessage combatMessage)
    {
        // Debug.Log(combatMessage);

        /*
        (var element, var controller) = CreateCombatResolution(combatMessage);

        doc.rootVisualElement.Add(element);
        */
        var data = new CombatResolutionController.SnapshotData(combatMessage.Resolver, combatMessage.Messages);
        dataList.Add(data);
    }

    (VisualElement, CombatResolutionController) CreateCombatResolution(CombatResolutionController.SnapshotData data)
    {
        var controller = new CombatResolutionController()
        {
            SubCombatLisyEntryTemplate = SubCombatLisyEntryTemplate,
            // StrengthStatsRowTemplate= StrengthStatsRowTemplate,
            FixedItemHeight = FixedItemHeight
        };
        var element = CombatResolutionTemplate.Instantiate();
        controller.SetVisualElement(element);
        controller.Sync(data);

        return (element, controller);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
