using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Figurine : MonoBehaviour // The name is borrowed from Tabletop Simulator
{
    public TMP_Text text;
    public UnityEngine.UI.Image image;

    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Sync(Sprite sprite, string label)
    {
        image.sprite = sprite;
        text.text = label;
    }
}
