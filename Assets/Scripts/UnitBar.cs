using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YYZ.BlackArmy.Model;

public class UnitBar : MonoBehaviour
{
    public TMP_Text detachmentNameText;
    public UnityEngine.UI.Image detachmentLeaderImage;
    public GameObject strengthContainer;
    public TMP_Text commanderNameText;
    public TMP_Text commanderStrategicText;
    public TMP_Text commanderOperationalText;
    public TMP_Text commanderGurrillaText;
    public TMP_Text commanderTacticalText;
    public TMP_Text statesText;
    public TMP_Text commandersStatsText;
    public GameObject SubCommanderContainer;

    public void Sync(Detachment detachment)
    {
        gameObject.SetActive(true);

        var d = detachment;

        detachmentNameText.text = d.Name;
        detachmentLeaderImage.sprite = Helpers.GetSprite(detachment);
        //

        Debug.Log($"UnitBar sync: {detachment}");
    }

    public void Close() => gameObject.SetActive(false);
    public void OpenWiki()
    {
        Debug.Log("OpenWiki");
    }

    public void OnMerge()
    {
        Debug.Log("OnMerge");
    }

    public void OnTransfer()
    {
        Debug.Log("OnTransfer");
    }

    public void OnRuleOfEngamentChanged()
    {
        Debug.Log("OnRuleOfEngamentChanged");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
