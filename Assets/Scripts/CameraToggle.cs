using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    public List<GameObject> toggleObjects;

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
        foreach (var obj in toggleObjects)
            obj.SetActive(!obj.activeSelf);
    }
}
