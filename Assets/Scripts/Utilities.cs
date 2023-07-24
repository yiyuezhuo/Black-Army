using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
}