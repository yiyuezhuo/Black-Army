using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadManager))]
public class RoadManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Debug.Log("TEST");
        var manager = (RoadManager)target;

        DrawDefaultInspector();

        // EditorGUILayout.HelpBox("HelpBox", MessageType.Info);
        // EditorGUILayout.IntField("Test", 10);

        if(GUILayout.Button("Normalize Editor Roads"))
        {
            foreach(Transform t in manager.EditorRoads.transform)
            {
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, 0);
                EditorUtility.SetDirty(t);
            }
        }
    }
}
