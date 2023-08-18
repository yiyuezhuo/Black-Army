using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
// using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
// using UnityEngine.EventSystems;


public class DraggableControl: MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    bool dragging = false;
    Vector2 lastPosition;

    public void OnPointerDown(PointerEventData ev)
    {
        // Debug.Log("PointerDown");
        dragging = true;
        lastPosition = ev.position;
    }


    public void OnPointerUp(PointerEventData ev)
    {
        // Debug.Log("PointerUp");
        dragging = false;
    }

    public void OnPointerMove(PointerEventData ev)
    {
        // Debug.Log("PointerMove");
        if (dragging)
        {
            var delta = ev.position - lastPosition;
            lastPosition = ev.position;
            transform.position += new Vector3(delta.x, delta.y, 0);
        }
    }

}

public class WikiDialog : DraggableControl
{
    public TMP_InputField urlField;

    private void Awake()
    {
        Provider.tryOpenWiki += (object _, string s) => Open(s); // TODO: refactor
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false); // TODO: refactor
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open(string s)
    {
        gameObject.SetActive(true);
        urlField.text = s;
    }

    public void OnConfirm()
    {
        Application.OpenURL(urlField.text);
        gameObject.SetActive(false);
    }

    public void OnCancel() => gameObject.SetActive(false);
}
