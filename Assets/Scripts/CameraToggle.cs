using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    public GameObject control1;
    public GameObject control2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle(bool _)
    {
        control1.SetActive(!control1.activeSelf);
        control2.SetActive(!control2.activeSelf);
    }
}
