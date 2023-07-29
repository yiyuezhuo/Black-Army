using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(HexMap))]
public class HexMapEditor : Editor
{
    // void OnDrawGizmos()
    void OnSceneGUI()
    {
        // Debug.Log("OnDrawGizmos");
        var hexMap = (HexMap)target;
        // Handles.Label(hexMap.transform.position, "Label");

        if(hexMap.drawEditorCoordinates)
        {
            var tilemap = hexMap.GetComponent<Tilemap>();
            for(var x= tilemap.cellBounds.xMin; x<tilemap.cellBounds.xMax; x++)
            {
                for(var y=tilemap.cellBounds.yMin; y<tilemap.cellBounds.yMax; y++)
                {
                    var cellIdx = new Vector3Int(x, y, 0);
                    var tile = tilemap.GetTile(cellIdx);
                    if(tile != null)
                    {
                        var worldCoords = tilemap.CellToWorld(cellIdx); // Or call it from Grid?
                        Handles.Label(worldCoords, $"{cellIdx.x},{cellIdx.y}");
                    }
                }
            }
        }
    }
}