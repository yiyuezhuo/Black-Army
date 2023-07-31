using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using YYZ.BlackArmy.Model;

public class DetachmentLeaderImage : MonoBehaviour, IPointerClickHandler
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
        // Application.OpenURL(leader.Wiki);
        Provider.OpenWiki(leader.Wiki);
    }

}
