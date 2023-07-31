using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using YYZ.BlackArmy.Model;

public class SubLeaderImage : MonoBehaviour, IPointerClickHandler
{
    public Leader leader;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Debug.Log($"{pointerEventData}, {leader}");
        // leader.Wiki
        // Application.OpenURL(leader.Wiki);
    }
}
