using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using YYZ.BlackArmy.Model;

public class CombatResolutionReporter : MonoBehaviour
{
    public int FixedItemHeight = 54;

    public VisualTreeAsset SubCombatLisyEntryTemplate;
    public VisualTreeAsset CombatResolutionTemplate;
    // public VisualTreeAsset StrengthStatsRowTemplate;
    public VisualTreeAsset TabsTemplate;

    UIDocument doc;

    List<CombatResolutionController.SnapshotData> dataList = new();

    public bool Show = true;

    // Start is called before the first frame update
    void Awake()
    {
        doc = GetComponent<UIDocument>();
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
        var data = new CombatResolutionController.SnapshotData(combatMessage.Resolver, combatMessage.Messages);
        dataList.Add(data);
    }

    public void Flush()
    {
        if(Show)
        {
            foreach (var group in dataList.GroupBy(data => data.LocationName))
            {
                (var tabElement, var controller) = CreateCombatResolutionTabs(group);
                doc.rootVisualElement.Add(tabElement);
            }
        }

        dataList.Clear();
    }

    public void ToggleShow(bool show) => Show = show;

    (VisualElement, TabsController) CreateCombatResolutionTabs(IEnumerable<CombatResolutionController.SnapshotData> group)
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

        return (tabElement, controller);
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
