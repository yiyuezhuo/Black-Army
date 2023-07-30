using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using YYZ.BlackArmy.Model;

public class Figurine : MonoBehaviour, IPointerClickHandler // The name is borrowed from Tabletop Simulator
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Sync(Sprite sprite, string label)
    {
        image.sprite = sprite;
        text.text = label;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        // Debug.Log($"{pointerEventData}, {hex}, {side}");
        clicked.Invoke(hex, side);
    }
}
