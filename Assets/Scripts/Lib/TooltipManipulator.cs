// https://stefansigl.wordpress.com/2020/09/21/uitoolkit-runtime-tool-tips-made-easy/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TooltipManipulator : Manipulator
{

    private VisualElement element;
    private Label label;
    public TooltipManipulator()
    {
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(MouseIn);
        target.RegisterCallback<MouseOutEvent>(MouseOut);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(MouseIn);
        target.UnregisterCallback<MouseOutEvent>(MouseOut);
    }

    private void MouseIn(MouseEnterEvent e)
    {
        if (element == null)
        {
            element = new VisualElement();
            element.style.backgroundColor = Color.blue;
            element.style.position = Position.Absolute;
            // label = new Label(this.target.tooltip);
            label = new Label();
            label.style.color = Color.white;

            element.Add(label);

            this.target.panel.visualTree.Add(element);
            // https://forum.unity.com/threads/get-manipulator-targets-root-element.1231668/
            // var root = (VisualElement)UiHelper.FindRootElement(this.target);
            // root.Add(element);
        }
        // Support draggable
        element.style.left = this.target.worldBound.center.x + this.target.transform.position.x;
        element.style.top = this.target.worldBound.yMin + this.target.transform.position.y;
        label.text = this.target.tooltip;
        element.style.visibility = Visibility.Visible;
        element.BringToFront();
    }

    private void MouseOut(MouseOutEvent e)
    {
        element.style.visibility = Visibility.Hidden;
    }
}