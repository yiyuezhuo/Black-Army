using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadManager))]
public class RoadManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var manager = (RoadManager)target;

        if (GUILayout.Button("Normalize Editor Roads"))
        {
            Utilities.SetZIndex(manager.EditorRoads.GetComponentsInChildren<LineRenderer>(), 0);
            // SetZIndex(manager.EditorRoads.GetComponentsInChildren<LineRenderer>());
            /*
            foreach(var render in manager.EditorRoads.GetComponentsInChildren<LineRenderer>())
            {
                var posArr = new Vector3[render.positionCount];
                render.GetPositions(posArr);
                for (var i = 0; i < posArr.Length; i++)
                    posArr[i].z = 0;
                render.SetPositions(posArr);

                EditorUtility.SetDirty(render);
            }
            */
        }

        // Vector3Int PositionArrayValueToCell(Vector3 x) => manager.gridLayout.WorldToCell(x )

        if(GUILayout.Button("Generate Roads"))
        {
            GenerateRoads();
        }

        if(GUILayout.Button("Set Z-Index for Generated Roads"))
        {
            Utilities.SetZIndex(manager.GeneratedRoads.GetComponentsInChildren<LineRenderer>(), manager.ZIndex);
        }

        if(GUILayout.Button("Sync Ordering Layer"))
        {
            var sortingLayerID = manager.GeneratedRoadPrefab.GetComponent<LineRenderer>().sortingLayerID;
            foreach (var renderer in manager.GeneratedRoads.GetComponentsInChildren<LineRenderer>())
            {
                renderer.sortingLayerID = sortingLayerID;
            }
        }

        /*
        if(GUILayout.Button("Test"))
        {
            var connectionSet = new HashSet<(Vector3Int, Vector3Int)>();
            var x = new Vector3Int(1, 1, 1);
            var y = new Vector3Int(1, 1, 1);
            connectionSet.Add((x, x));
            connectionSet.Add((x, x));
            connectionSet.Add((y, y));
            connectionSet.Add((x, y));
            Debug.Log($"x==y:{x==y}, x.Equals(y):{x.Equals(y)}, connectionSet.Count:{connectionSet.Count}");
            // x==y:True, x.Equals(y):True, connectionSet.Count:1
        }
        */
    }

    void GenerateRoads()
    {
        var manager = (RoadManager)target;

        var connectionSet = new HashSet<(Vector3Int, Vector3Int)>();
        foreach (var render in manager.EditorRoads.GetComponentsInChildren<LineRenderer>())
        {
            var prev = manager.gridLayout.WorldToCell(render.GetPosition(0) + render.transform.localPosition);
            for (var i = 1; i < render.positionCount; i++)
            {
                var current = manager.gridLayout.WorldToCell(render.GetPosition(i) + render.transform.localPosition);
                if (current == prev)
                    continue;

                var t = (prev, current);
                connectionSet.Add(t);

                prev = current;
            }
        }

        foreach (Transform t in manager.GeneratedRoads.transform)
        {
            Debug.Log($"Destroying {t}");
            DestroyImmediate(t.gameObject); // In Editor mode, Destroy can not be called.
                                            // DestroyI(t.gameObject);
        }

        foreach ((var leftCell, var rightCell) in connectionSet)
        {
            var road = Instantiate(manager.GeneratedRoadPrefab, manager.GeneratedRoads.transform);
            var left = manager.gridLayout.CellToWorld(leftCell);
            var right = manager.gridLayout.CellToWorld(rightCell);
            // Debug.Log($"Draw: {left} to {right}");
            // road.transform.localPosition = left;
            var render = road.GetComponent<LineRenderer>();
            render.SetPosition(0, left);
            render.SetPosition(1, right);
            // render.SetWidth()
        }
    }
}

