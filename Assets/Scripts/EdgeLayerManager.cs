using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EdgeLayerManager : MonoBehaviour
{
    public GameObject edgePrefab;
    public GridLayout gridLayout;
    public float ZIndex;
    public float LineWidth = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public static class FlatTopShiftedSystem
{
    public static Vector2[] xys;

    static FlatTopShiftedSystem()
    {
        xys = new Vector2[6];
        for(var i=0; i<6; i++)
        {
            var r = Mathf.PI / 3 * i;
            xys[i] = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }
    }

    public static Vector2 TextureAdjustCoef = new Vector2(1, 2 / Mathf.Sqrt(3));
}
