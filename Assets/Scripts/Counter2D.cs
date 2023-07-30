using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using YYZ.BlackArmy.Model;

public class Counter2D : MonoBehaviour
{
    public TMP_Text text;
    public UnityEngine.UI.Image image;

    // public Detachment detachment;

    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
    }

    public void Sync(string s, Sprite sprite)
    {
        text.text = s;
        image.sprite = sprite;
    }

    /*
    void Sync()
    {
        text.text = detachment.GetTotalManpower().ToString();
        var sprite = Resources.Load<Sprite>("Flags/" + detachment.Side.Name);
        image.sprite = sprite;
    }
    */

    // Update is called once per frame
    void Update()
    {
        
    }
}
