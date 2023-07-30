using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EdgeLayerManager))]
public class EdgeLayerManagerEditor : Editor
{
    public void OnSceneGUI()
    {
        Handles.BeginGUI();
        EditorGUILayout.HelpBox("When Edge Layer is Selected, left-clicking will create an edge instead of selecting an object", MessageType.Info);
        Handles.EndGUI();

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log("clicked");

            var t = (EdgeLayerManager)target;
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            var cell = t.gridLayout.WorldToCell(ray.origin);
            var cellCenter = t.gridLayout.CellToWorld(cell);
            var delta = ray.origin - cellCenter;
            var radian = Mathf.Atan2(delta.y, delta.x);

            var firstIdx = (int)Mathf.Floor(radian / (Mathf.PI / 3));
            firstIdx = firstIdx < 0 ? firstIdx + 6 : firstIdx; // C#/C++ "mod" is not consist with Python when input is negative
            var secondIdx = (firstIdx + 1) % 6;

            // Debug.Log($"ray={ray}, cell={cell}, cellCenter={cellCenter}, delta={delta}, radian={radian}, firstIdx={firstIdx}, secondIdx={secondIdx}");
            Debug.Log($"radian={radian}, firstIdx={firstIdx}, secondIdx={secondIdx}");

            var cellSize = t.gridLayout.cellSize / 2;
            var firstPos = GetHexVertex(cellCenter, cellSize, firstIdx);
            var secondPos = GetHexVertex(cellCenter, cellSize, secondIdx);
            var center = (firstPos + secondPos) / 2;

            var destroyAny = false;
            foreach (Transform tra in t.transform)
            {
                var subRender = tra.GetComponent<LineRenderer>();
                var subCenter = (subRender.GetPosition(0) + subRender.GetPosition(1)) / 2;
                var diff = (Vector2)subCenter - center;
                if (diff.magnitude < 1e-4)
                {
                    DestroyImmediate(tra.gameObject);
                    destroyAny = true;
                }
            }
            if (!destroyAny)
            {
                // var obj = Instantiate(t.edgePrefab, t.transform); // TODO: Use `PrefabUtility.InstantiatePrefab` ?
                var obj = PrefabUtility.InstantiatePrefab(t.edgePrefab, t.transform) as GameObject; // TODO: Use `PrefabUtility.InstantiatePrefab` ?
                var render = obj.GetComponent<LineRenderer>();
                render.positionCount = 2;

                render.SetPosition(0, GetHexVertex(cellCenter, cellSize, firstIdx));
                render.SetPosition(1, GetHexVertex(cellCenter, cellSize, secondIdx));
            }

            e.Use();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (EdgeLayerManager)target;

        if (GUILayout.Button("Set Z-Index"))
        {
            Debug.Log($"Set ZIndex to {t.ZIndex}");
            Utilities.SetZIndex(t.GetComponentsInChildren<LineRenderer>(), t.ZIndex);
            // Utilities.SetZIndex();
        }

        if (GUILayout.Button("Set Line Width"))
        {
            foreach (var renderer in t.GetComponentsInChildren<LineRenderer>())
            {
                // renderer.SetWidth(t.LineWidth, t.LineWidth);
                renderer.startWidth = t.LineWidth;
                renderer.endWidth = t.LineWidth;
            }
        }

        if(GUILayout.Button("Sync Order Layer"))
        {
            var layerID = t.edgePrefab.GetComponent<LineRenderer>().sortingLayerID;
            foreach(var renderer in t.GetComponentsInChildren<LineRenderer>())
            {
                renderer.sortingLayerID = layerID;
            }
        }
    }

    Vector2 GetHexVertex(Vector2 center, Vector2 cellSize, int idx)
    {
        // Debug.Log(FlatTopShiftedSystem.xys);
        /*
        return new Vector2(
            center.x + cellSize.x * FlatTopShiftedSystem.xys[idx].x,
            center.y + cellSize.y * FlatTopShiftedSystem.xys[idx].y
        );
        */
        return center + cellSize * FlatTopShiftedSystem.xys[idx] * FlatTopShiftedSystem.TextureAdjustCoef;
    }
}