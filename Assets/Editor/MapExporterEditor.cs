using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

[CustomEditor(typeof(MapExporter))]
public class MapExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Export Map Edges"))
        {
            ExportMapEdges();
        }

        if (GUILayout.Button("Export Map Hexes"))
        {
            ExportMapHexes();
        }
    }

    public class Edge
    {
        public bool River;
        public bool Railroad;
        public bool CountryBoundary;
    }

    public class EdgeRow
    {
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public int DestinationX { get; set; }
        public int DestinationY { get; set; }
        public bool River { get; set; }
        public bool Railroad { get; set; }
        public bool CountryBoundary { get; set; }
    }

    void ExportMapEdges()
    {
        var edgeMap = GetMapEdges();

        var rows = new List<EdgeRow>();
        foreach(((var sx, var sy, var dx, var dy), var edge) in edgeMap)
        {
            var row = new EdgeRow() { SourceX = sx, SourceY = sy, DestinationX = dx, DestinationY = dy, River = edge.River, Railroad = edge.Railroad, CountryBoundary = edge.CountryBoundary };
            rows.Add(row);
        }

        // rows.Sort();

        // Debug.Log(Utilities.GetCsvText(rows));
        Utilities.ExportAsCsv<EdgeRow>(rows, "Edges.csv");
    }

    Dictionary<(int, int, int, int), Edge> GetMapEdges()
    {
        var mapExporter = (MapExporter)target;
        var tilemap = mapExporter.tilemap;

        var edgeMap = new Dictionary<(int, int, int,int), Edge>();

        foreach ((var x, var y, var cellIdx, var tile) in IterHexes(tilemap)) // TODO: Use traditional offset array?
        {
            var worldPos = mapExporter.tilemap.CellToWorld(cellIdx);
            for (var i=0; i<mapExporter.neighborSamplePoints; i++)
            {
                var p = ((float)i) / mapExporter.neighborSamplePoints;
                var dx = Mathf.Cos(p * 2 * Mathf.PI) * mapExporter.neighborRaycastRadius;
                var dy = Mathf.Sin(p * 2 * Mathf.PI) * mapExporter.neighborRaycastRadius;

                var testPos = worldPos + new Vector3(dx, dy, 0);
                var testCellIdx = tilemap.WorldToCell(testPos);
                if(cellIdx != testCellIdx)
                {
                    var key = (x, y, testCellIdx.x, testCellIdx.y);
                    if (!edgeMap.ContainsKey(key))
                    {
                        edgeMap[key] = new Edge();
                    }
                }
            }
        }

        Debug.Log($"edgeMap.Count={edgeMap.Count}");

        var riverKeys = GetEdgeRelatedHexes(tilemap, mapExporter.riverContainer.transform, mapExporter.edgeRaycastRadius).ToList();

        foreach (var key in riverKeys)
            edgeMap[key].River = true;

        Debug.Log($"riverKeys.Count={riverKeys.Count}");

        var countryBoundaryKeys = GetEdgeRelatedHexes(tilemap, mapExporter.countryBoundaryContainer.transform, mapExporter.edgeRaycastRadius).ToList();

        foreach (var key in countryBoundaryKeys)
            edgeMap[key].CountryBoundary = true;

        Debug.Log($"countryBoundaryKeys.Count={countryBoundaryKeys.Count}");

        var railRoadKeys = GetPathRelatedHexes(tilemap, mapExporter.roadContainer.transform).ToList();

        foreach (var key in railRoadKeys)
            edgeMap[key].Railroad = true;

        Debug.Log($"railRoadKeys.Count={railRoadKeys.Count}");

        return edgeMap;
    }

    static IEnumerable<(int, int, int, int)> GetEdgeRelatedHexes(Tilemap tilemap, Transform transform, float edgeRaycastRadius)
    {
        foreach (Transform t in transform)
        {
            var lineRenderer = t.GetComponent<LineRenderer>();
            var p0 = lineRenderer.GetPosition(0);
            var p1 = lineRenderer.GetPosition(1);
            var center = (p0 + p1) / 2;
            var d = (p0 - p1).normalized * edgeRaycastRadius;
            var dPerp0 = new Vector3(d.y, -d.x, 0);
            var dPerp1 = -dPerp0;

            foreach (var key in IterHexKeyIfHas(tilemap, center + dPerp0, center + dPerp1))
                yield return key;
            /*
            var cellIdx0 = tilemap.WorldToCell(center + dPerp0);
            var cellIdx1 = tilemap.WorldToCell(center + dPerp1);
            var tile0 = tilemap.GetTile(cellIdx0);
            var tile1 = tilemap.GetTile(cellIdx1);
            if (tile0 != null && tile1 != null)
            {
                // var key = (cellIdx0.x, cellIdx0.y, cellIdx1.x, cellIdx1.y);
                yield return (cellIdx0.x, cellIdx0.y, cellIdx1.x, cellIdx1.y);
                yield return (cellIdx1.x, cellIdx1.y, cellIdx0.x, cellIdx0.y);
            }
            */
        }
    }

    static IEnumerable<(int, int, int, int)> GetPathRelatedHexes(Tilemap tilemap, Transform transform)
    {
        foreach(Transform t in transform)
        {
            var lineRenderer = t.GetComponent<LineRenderer>();
            var p0 = lineRenderer.GetPosition(0);
            var p1 = lineRenderer.GetPosition(1);

            foreach (var key in IterHexKeyIfHas(tilemap, p0, p1))
                yield return key;
            /*
            var cellIdx0 = tilemap.WorldToCell(p0);
            var cellIdx1 = tilemap.WorldToCell(p1);

            var tile0 = tilemap.GetTile(cellIdx0);
            var tile1 = tilemap.GetTile(cellIdx1);

            if (tile0 != null && tile1 != null)
            {
                // var key = (cellIdx0.x, cellIdx0.y, cellIdx1.x, cellIdx1.y);
                yield return (cellIdx0.x, cellIdx0.y, cellIdx1.x, cellIdx1.y);
                yield return (cellIdx1.x, cellIdx1.y, cellIdx0.x, cellIdx0.y);
            }
            */
        }
    }

    static IEnumerable<(int, int, int, int)> IterHexKeyIfHas(Tilemap tilemap, Vector3 p0, Vector3 p1)
    {
        var cellIdx0 = tilemap.WorldToCell(p0);
        var cellIdx1 = tilemap.WorldToCell(p1);

        var tile0 = tilemap.GetTile(cellIdx0);
        var tile1 = tilemap.GetTile(cellIdx1);

        if (tile0 != null && tile1 != null)
        {
            // var key = (cellIdx0.x, cellIdx0.y, cellIdx1.x, cellIdx1.y);
            yield return (cellIdx0.x, cellIdx0.y, cellIdx1.x, cellIdx1.y);
            yield return (cellIdx1.x, cellIdx1.y, cellIdx0.x, cellIdx0.y);
        }
    }

    static IEnumerable<(int, int, Vector3Int, TileBase)> IterHexes(Tilemap tilemap)
    {
        for (var x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
        {
            for (var y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
            {
                var cellIdx = new Vector3Int(x, y, 0);
                var tile = tilemap.GetTile(cellIdx);
                if (tile != null)
                {
                    yield return (x, y, cellIdx, tile);
                }
            }
        }
    }

    void ExportMapHexes()
    {
        var tilemap = ((MapExporter)target).tilemap;

        var rows = new List<HexRow>();

        foreach ((var x, var y, var cellIdx, var tile) in IterHexes(tilemap))
        {
            var row = new HexRow() { X = x, Y = y, Type = tile.name };
            rows.Add(row);
        }

        /*
        for (var x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
        {
            for (var y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
            {
                var cellIdx = new Vector3Int(x, y, 0);
                var tile = tilemap.GetTile(cellIdx);
                if (tile != null)
                {
                    var row = new HexRow() { X = x, Y = y, Type = tile.name };
                    rows.Add(row);
                    // var worldCoords = tilemap.CellToWorld(cellIdx); // Or call it from Grid?
                    // Handles.Label(worldCoords, $"{cellIdx.x},{cellIdx.y}");
                }
            }
        }
        */

        Utilities.ExportAsCsv<HexRow>(rows, "Hexes.csv");
    }

    public class HexRow
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; }
    }

}
