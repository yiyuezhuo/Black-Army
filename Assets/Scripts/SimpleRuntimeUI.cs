using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DragManipulatorFree: DragManipulator
{
    public override void ResetPosition() { }
}

public class SimpleRuntimeUI : MonoBehaviour
{
    private Button _button;
    private Toggle _toggle;

    private int _clickCount;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        // VisualElementExtensions.AddManipulator()

        // uiDocument.AddManipulator(new DragManipulator());
        // uiDocument.Add

        _button = uiDocument.rootVisualElement.Q("button") as Button;
        _toggle = uiDocument.rootVisualElement.Q("toggle") as Toggle;

        var label = uiDocument.rootVisualElement.Q("DraggableLabel") as Label;
        var div = uiDocument.rootVisualElement.Q("background") as VisualElement;

        /*
        var element = div;

        element.AddManipulator(new DragManipulatorFree());
        element.RegisterCallback<DropEvent>(evt =>
          Debug.Log($"{evt.target} dropped on {evt.droppable}"));
        

        var element = label;

        element.AddManipulator(new DragManipulator());
        element.RegisterCallback<DropEvent>(evt =>
          Debug.Log($"{evt.target} dropped on {evt.droppable}"));
        */
        div.AddManipulator(new SimpleDraggingManipulator());


        _button.RegisterCallback<ClickEvent>(PrintClickMessage);

        var _inputFields = uiDocument.rootVisualElement.Q("input-message");
        _inputFields.RegisterCallback<ChangeEvent<string>>(InputMessage);
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(PrintClickMessage);
    }

    private void PrintClickMessage(ClickEvent evt)
    {
        ++_clickCount;

        Debug.Log($"{"button"} was clicked!" +
                (_toggle.value ? " Count: " + _clickCount : ""));
    }

    public static void InputMessage(ChangeEvent<string> evt)
    {
        Debug.Log($"{evt.newValue} -> {evt.target}");
    }
}