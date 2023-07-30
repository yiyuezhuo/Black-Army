using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class DateBar : MonoBehaviour
{
    /*
    public int beginYear;
    public int beginMonth;
    public int beginDay;

    DateTime currentDate;
    TimeSpan stepSize = TimeSpan.FromDays(1);
    */

    public GameManager gameManager;

    TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Sync();
    }

    public void Sync()
    {
        text.text = gameManager.state.CurrentDateTime.ToString("dddd, dd MMMM yyyy");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
