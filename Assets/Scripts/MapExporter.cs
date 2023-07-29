using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapExporter : MonoBehaviour
{
    public Tilemap tilemap;

    public GameObject riverContainer;
    public GameObject roadContainer;
    public GameObject countryBoundaryContainer;

    public float neighborRaycastRadius = 0.5f;
    public int neighborSamplePoints = 12;
    public float edgeRaycastRadius = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
