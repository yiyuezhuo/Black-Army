using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using System.Linq;

public class MoveLine : MonoBehaviour
{
    LineRenderer line;

    public GridLayout grid;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        // Debug.Log($"line={line}");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Sync(Vector3[] points, float percent)
    {
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.material.SetFloat("_Percent", percent); // TODO: PiecewiseLine has some rendering ordering issue I don't have the time to fix.
    }

    public void OnDetachmentSelected(Detachment detachment)
    {
        if (detachment.MovingState == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        var path = new List<Hex>() { detachment.Hex, detachment.MovingState.CurrentTarget};
        path.AddRange(detachment.MovingState.Waypoints);

        var positions = path.Select(hex => grid.CellToWorld(new Vector3Int(hex.X, hex.Y, 0))).ToArray();
        var percent = detachment.MovingState.CurrentCompleted / positions.Length;

        Sync(positions, percent);
    }

    public void OnCurrentDetachmentMovingStateChanged(Detachment detachment) => OnDetachmentSelected(detachment);

    public void OnDetachmentDeselected()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
