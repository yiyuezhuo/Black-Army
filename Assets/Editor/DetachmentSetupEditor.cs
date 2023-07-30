using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Text;

[CustomEditor(typeof(DetachmentSetup))]
public class DetachmentSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Export"))
        {
            Export();
        }

        if(GUILayout.Button("Import"))
        {
            Debug.Log("Not Implemented Yet");
        }
    }

    void Export()
    {
        var setup = (DetachmentSetup)target;

        var rows = new List<DetachmentRow>();

        foreach(Transform t in setup.transform)
        {
            var cellIdx = setup.grid.WorldToCell(t.transform.position);
            var marker = t.GetComponent<DetachmentSetupMarker>();
            var row = new DetachmentRow() { ID = t.gameObject.name, X = cellIdx.x, Y = cellIdx.y, Side = marker.Side };
            rows.Add(row);
        }

        Utilities.ExportAsCsv<DetachmentRow>(rows, "Detachments.csv");
    }

    public class DetachmentRow
    {
        public string ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Side { get; set; }
    }

}
