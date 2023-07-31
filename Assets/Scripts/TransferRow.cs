using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YYZ.BlackArmy.Model;

public class TransferRow : MonoBehaviour
{
    public TMP_Text typeNameText;
    public TMP_Text sourceQuantityText;
    public UnityEngine.UI.Slider slider;
    public TMP_Text destinationQuantityText;

    int srcBase;
    int dstBase;
    int q;

    public void Sync(string name, int srcBase, int dstBase)
    {
        this.srcBase = srcBase;
        this.dstBase = dstBase;
        this.q = 0;

        slider.minValue = 0;
        slider.maxValue = srcBase;

        Sync(0);

        typeNameText.text = name;
    }

    void Sync(int q)
    {
        slider.value = q;
        Sync();
    }

    void Sync()
    {
        sourceQuantityText.text = (srcBase - q).ToString();
        destinationQuantityText.text = (dstBase + q).ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderValueChanged(float p)
    {
        q = (int)p;
        Sync();
    }
}
