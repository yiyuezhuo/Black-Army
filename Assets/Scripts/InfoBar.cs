using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InfoBar : MonoBehaviour
{
    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();

        Provider.messaged += OnMessaged;
    }

    public void OnMessaged(object _, string s)
    {
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("HH:mm:ss");
        text.text = $"{formattedDateTime}: {s}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
