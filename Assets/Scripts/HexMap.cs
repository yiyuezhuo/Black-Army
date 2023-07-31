using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using YYZ.BlackArmy.Model;

public class HexMap : MonoBehaviour
{
    public bool drawEditorCoordinates = false;

    Vector3 downMousePosition;
    public float rightClickSelectionSensitivity = 2;

    public UnityEvent<Hex> hexRightClicked;

    Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            downMousePosition = Input.mousePosition; // screen
        }
        if(Input.GetMouseButtonUp(1))
        {
            var diff = downMousePosition - Input.mousePosition;
            if(diff.magnitude < rightClickSelectionSensitivity)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(Vector3.forward, Vector3.zero);
                if (plane.Raycast(ray, out var distance))
                {
                    var hitPoint = ray.GetPoint(distance);
                    var cellIdx = tilemap.WorldToCell(hitPoint);
                    if(tilemap.GetTile(cellIdx) != null)
                    {
                        var hex = Provider.GetHex(cellIdx.x, cellIdx.y);
                        Debug.Log($"downMousePosition={downMousePosition},Input.mousePosition={Input.mousePosition},hitPoint={hitPoint},cellIdx={cellIdx},hex={hex}");
                        hexRightClicked.Invoke(hex);
                    }
                }
            }
        }
    }
}
