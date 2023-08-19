// Reference:
// https://gist.github.com/shanecelis/b6fb3fe8ed5356be1a3aeeb9e7d2c145

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleDraggingManipulator : IManipulator
{
    VisualElement _target;
    bool dragging = false;
    Vector3 lastPosition;

    public VisualElement target 
    { 
        get => _target; 
        set
        {
            if(_target != null && _target != value)
            {
                _target.UnregisterCallback<PointerDownEvent>(PointerDown);
                _target.UnregisterCallback<PointerUpEvent>(PointerUp);
                _target.UnregisterCallback<PointerMoveEvent>(PointerMove);
            }
            _target = value;
            _target.RegisterCallback<PointerDownEvent>(PointerDown);
            _target.RegisterCallback<PointerUpEvent>(PointerUp);
            _target.RegisterCallback<PointerMoveEvent>(PointerMove);
        }
    }

    public void PointerDown(PointerDownEvent ev)
    {
        // Debug.Log("PointerDown");
        dragging = true;
        lastPosition = ev.position;

        target.BringToFront();
    }

    public void PointerUp(PointerUpEvent ev)
    {
        // Debug.Log("PointerUp");
        dragging = false;
    }

    public void PointerMove(PointerMoveEvent ev)
    {
        // Debug.Log("PointerMove");
        if(dragging)
        {
            var delta = ev.position - lastPosition;
            lastPosition = ev.position;
            target.transform.position += delta;
        }
    }
}
