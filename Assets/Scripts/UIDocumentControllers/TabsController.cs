using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TabsController
{
    VisualElement root;
    VisualElement tabRow;
    VisualElement content;

    public void SetVisualElement(VisualElement element)
    {
        root = element;
        tabRow = root.Q("TabsRow");
        content = root.Q("Content");
    }

    Dictionary<Label, VisualElement> tab2Container = new();
    List<Label> tabLabels = new();
    Label selectingLabel;

    public void OnLabelClicked(ClickEvent evt)
    {
        var selectedLabel = evt.currentTarget as Label;
        if(selectingLabel != selectedLabel)
        {
            if(selectingLabel != null)
                DeselectLabel(selectingLabel);
            SelectLabel(selectedLabel);
        }
    }

    public void SetData(IEnumerable<(string, VisualElement)> tabs)
    {
        tabRow.Clear();
        content.Clear();
        tab2Container.Clear();
        tabLabels.Clear();

        foreach ((var tabName, var element) in tabs)
        {
            var label = new Label();
            label.text = tabName;
            label.AddToClassList("tab");
            label.RegisterCallback<ClickEvent>(OnLabelClicked);
            // label.RegisterCallback<ClickEvent>(OnLabelClicked, TrickleDown.TrickleDown);
            tabRow.Add(label);

            var container = new VisualElement();
            container.style.position = Position.Absolute;
            container.AddToClassList("unselectedContent");
            container.Add(element);
            content.Add(container);
            
            tab2Container[label] = container;
            tabLabels.Add(label);
        }

        SelectLabel(tabLabels[0]);
    }

    void SelectLabel(Label label)
    {
        label.AddToClassList("selectedTab");
        tab2Container[label].RemoveFromClassList("unselectedContent");
        selectingLabel = label;
    }

    void DeselectLabel(Label label)
    {
        label.RemoveFromClassList("selectedTab");
        tab2Container[label].AddToClassList("unselectedContent");
    }
}
