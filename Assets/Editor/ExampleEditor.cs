using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// A tiny custom editor for ExampleScript component
[CustomEditor(typeof(ExampleScript))]
public class ExampleEditor : Editor
{
    // Custom in-scene UI for when ExampleScript
    // component is selected.
    public void OnSceneGUI()
    {
        // Debug.Log("OnSceneGUI");

        var t = target as ExampleScript;
        var tr = t.transform;
        var pos = tr.position;
        // display an orange disc where the object is
        var color = new Color(1, 0.8f, 0.4f, 1);
        Handles.color = color;
        Handles.DrawWireDisc(pos, tr.up, 1.0f);
        // display object "value" in scene
        GUI.color = color;
        Handles.Label(pos, t.value.ToString("F1"));

        /*
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click");
            // Instantiate()
        }
        */

        Handles.BeginGUI();
        if (GUILayout.Button("Reset Area", GUILayout.Width(100)))
        {
            Debug.Log("Click custom button");
        }
        Handles.EndGUI();

        /*
        Event e = Event.current;
        e.Use();
        */

        /*
        Event e = Event.current;
        Debug.Log(e);
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // var t = target as ExampleScript;
                // GameObject newObject = new GameObject("NewObject");
                var newObject = Instantiate(t.Prefab);
                newObject.transform.position = hit.point;
                Selection.activeGameObject = newObject;
                e.Use();
            }
        }
        */
    }


    void Callback(SceneView sceneView)
    {
        // Debug.Log("Callback");
        Event e = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        // https://discussions.unity.com/t/disable-mouse-selection-in-editor-view/29618
        if (e.type == EventType.MouseDown && e.button == 0)
        {

            var t = target as ExampleScript;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            Debug.Log($"Click Begin: {ray}");

            var newObject = Instantiate(t.Prefab);
            newObject.transform.position = new Vector3(ray.origin.x, ray.origin.y, 0);

            /*
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Hit");
                var t = target as ExampleScript;
                // GameObject newObject = new GameObject("NewObject");
                var newObject = Instantiate(t.Prefab);
                newObject.transform.position = hit.point;
                Selection.activeGameObject = newObject;
                e.Use();
            }
            */
            // Debug.Log($"Click End {ray}| {hit}");

            e.Use();
        }
    }

    // Window has been selected
    public void OnEnable()
    {
        Debug.Log("OnEnable");
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.duringSceneGui -= Callback;

        // Add (or re-add) the delegate.
        SceneView.duringSceneGui += Callback;

        // HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    public void OnDestroy()
    {
        Debug.Log("OnDestroy");
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.duringSceneGui -= Callback;

        // HandleUtility.Repaint();
    }

}