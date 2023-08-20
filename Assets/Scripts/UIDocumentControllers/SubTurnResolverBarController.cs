using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SubTurnResolverBarController
{
    VisualElement visualElement;
    Label label;

    public void SetVisualElement(VisualElement visualElement)
    {
        this.visualElement = visualElement;
        label = visualElement.Q("SubTurnLabel") as Label;
    }

    public void SetData(int currentSubTurn, int totalSubTurn)
    {
        label.text = $"Sub Turn {currentSubTurn}/{totalSubTurn}";
    }
}
