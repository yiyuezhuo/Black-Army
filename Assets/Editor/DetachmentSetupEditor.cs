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

        var rows = new List<Row>();

        foreach(Transform t in setup.transform)
        {
            var cellIdx = setup.grid.WorldToCell(t.transform.position);
            var marker = t.GetComponent<DetachmentSetupMarker>();
            var row = new Row() { ID = t.gameObject.name, X = cellIdx.x, Y = cellIdx.y, Side = marker.Side };
            rows.Add(row);
        }

        /*
        var memoryStream = new MemoryStream(); // Though MemoryStream implements IDisposable, there're no actually resource to dispose, so we don't use using block here.

        using (var streamWriter = new StreamWriter(memoryStream))
        {
            using(var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
            }
        }

        var bytes = memoryStream.ToArray();
        var s = Encoding.UTF8.GetString(bytes);
        //  Debug.Log(s);
        
        var path = EditorUtility.SaveFilePanel("Export to", "", "Detachments.csv", "csv");
        File.WriteAllText(path, s);
        */
        Utilities.ExportAsCsv<Row>(rows, "Detachments.csv");
    }

    public class Row
    {
        public string ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Side { get; set; }
    }

}
