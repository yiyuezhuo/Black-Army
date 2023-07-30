using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Text;


public static class Utilities
{
    public static void SetZIndex(IEnumerable<LineRenderer> renders, float value)
    {
        foreach (var render in renders)
        {
            var posArr = new Vector3[render.positionCount];
            render.GetPositions(posArr);
            for (var i = 0; i < posArr.Length; i++)
                posArr[i].z = value;
            render.SetPositions(posArr);

            EditorUtility.SetDirty(render);
        }
    }

    public static string GetCsvText<T>(List<T> rows)
    {
        var memoryStream = new MemoryStream(); // Though MemoryStream implements IDisposable, there're no actually resource to dispose, so we don't use using block here.

        using (var streamWriter = new StreamWriter(memoryStream))
        {
            using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
            }
        }

        var bytes = memoryStream.ToArray();
        var s = Encoding.UTF8.GetString(bytes);
        return s;
    }

    public static void ExportAsCsv<T>(List<T> rows, string name)
    {
        var s = GetCsvText(rows);
        var path = EditorUtility.SaveFilePanel("Export to", "", name, "csv");
        if (path == "")
            Debug.Log("Cancel Save");
        else
            File.WriteAllText(path, s);
    }
}