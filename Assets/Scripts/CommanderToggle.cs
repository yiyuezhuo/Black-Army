using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CommanderToggle : MonoBehaviour
{
    public UnityEngine.UI.Text label;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Sync(string s)
    {
        label.text = s;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
