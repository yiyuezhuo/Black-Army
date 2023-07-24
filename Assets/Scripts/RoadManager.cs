using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadManager : MonoBehaviour
{
    public GameObject EditorRoads;
    public GameObject GeneratedRoads;
    public GridLayout gridLayout;
    public GameObject GeneratedRoadPrefab;
    public float ZIndex = -0.2f;

    // Start is called before the first frame update
    void Start()
    {
        EditorRoads.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
