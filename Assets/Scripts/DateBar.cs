using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class DateBar : MonoBehaviour
{
    public int beginYear;
    public int beginMonth;
    public int beginDay;

    DateTime currentDate;
    TimeSpan stepSize = TimeSpan.FromDays(1);

    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();

        currentDate = new DateTime(beginYear, beginMonth, beginDay);
        Sync();
    }

    void Sync()
    {
        text.text = currentDate.ToString("dddd, dd MMMM yyyy");
    }

    public void GotoNextStep()
    {
        currentDate += stepSize;
        Sync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
