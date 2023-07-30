using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using YYZ.BlackArmy.Model;

public class Counter2D : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text text;
    public UnityEngine.UI.Image image;

    public Hex hex;
    public Side side;

    public UnityEvent<Hex, Side> clicked;

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

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        // Debug.Log($"{pointerEventData}, {hex}, {side}");
        clicked.Invoke(hex, side);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
