using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SubCombatListEntryController
{
    public SideUI LeftUI;
    public SideUI RightUI;
    public VisualElement ResultSummary;

    public class SideUI
    {
        public VisualElement Flag;
        public Label SubCombatSummary;

        public SideUI(VisualElement root, string prefix)
        {
            Flag = root.Q(prefix + "Flag");
            SubCombatSummary = root.Q(prefix + "SubCombatSummary") as Label;
        }
    }

    public class SideData
    {
        public Sprite Flag;
        public int Committed;
        public int CommittedLost;
        public float SituationDelta;
        public float ChancePotentialDelta;
        public float ChanceBaselineDelta;
    }

    public class Data
    {
        public SideData Left;
        public SideData Right;
        public Color Tint;
        public Sprite CombatTypeSprite;
    }

    public void SetVisualElement(VisualElement visualElement)
    {
        LeftUI = new SideUI(visualElement, "Left");
        RightUI = new SideUI(visualElement, "Right");
        ResultSummary = visualElement.Q("ResultSummary");
    }

    public void SetData(Data data)
    {
        SetData(data.Left, LeftUI);
        SetData(data.Right, RightUI);
        ResultSummary.style.unityBackgroundImageTintColor = data.Tint;
        ResultSummary.style.backgroundImage = new StyleBackground(data.CombatTypeSprite);
    }

    public void SetData(SideData data, SideUI ui)
    {
        ui.Flag.style.backgroundImage = new StyleBackground(data.Flag);

        ui.SubCombatSummary.text = $@"Committed: {data.Committed} (-{data.CommittedLost})
Situation: {data.SituationDelta.ToString("P4")}
Chance: {data.ChancePotentialDelta}({data.ChanceBaselineDelta})";
    }
}
