using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InfoBar : MonoBehaviour
{
    TMP_Text text;

    string lastText;
    int t = 0;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();

        Provider.messaged += OnMessaged;
    }

    public void OnMessaged(object _, string s)
    {
        if(lastText != null && lastText == s)
        {
            t += 1;
            text.text = $"(x{t}) {s}";
        }
        else
        {
            t = 0;
            lastText = s;
            text.text = s;
        }
        /*
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("HH:mm:ss");
        text.text = $"{formattedDateTime}: {s}";
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
