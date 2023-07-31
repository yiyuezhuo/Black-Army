using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YYZ.BlackArmy.Model;
using System.Linq;
using UnityEngine.Events;

public class UnitBar : MonoBehaviour
{
    public TMP_Text detachmentNameText;
    public UnityEngine.UI.Image detachmentLeaderImage;
    public GameObject strengthContainer;
    public TMP_Text commanderNameText;
    public TMP_Text commanderStrategicText;
    public TMP_Text commanderOperationalText;
    public TMP_Text commanderGuerrillaText;
    public TMP_Text commanderTacticalText;
    public TMP_Text statesText;
    public TMP_Text commandersStatsText;
    public GameObject subCommanderContainer;

    public GameObject categoryStrengthItemPrefab;
    public GameObject commanderPanelPrefab;

    public UnityEvent<Detachment> detachmentTransferred;
    public UnityEvent closed;

    Detachment currentDetachment;

    public void Sync(Detachment detachment)
    {
        // gameObject.SetActive(true);

        // Debug.Log($"UnitBar sync: {detachment}");

        currentDetachment = detachment;

        detachmentNameText.text = detachment.Name;
        detachmentLeaderImage.sprite = Helpers.GetSprite(detachment);

        var leader = detachment.CurrentLeader;
        detachmentLeaderImage.GetComponent<DetachmentLeaderImage>().leader = leader;

        SyncStrengthContainer(detachment);

        commanderNameText.text = leader.Name;
        commanderStrategicText.text = $"Strategic: {leader.Strategic}";
        commanderOperationalText.text = $"Operational: {leader.Operational}";
        commanderGuerrillaText.text = $"Guerrilla: {leader.Guerrilla}";
        commanderTacticalText.text = $"Tactical: {leader.Tactical}";

        SyncStates(detachment);

        var tacticalSum = detachment.Leaders.Sum(l => l.Tactical);
        commandersStatsText.text = $"{detachment.Leaders.Count} Commanders, tactical sum:{tacticalSum}, modifier:+0%";

        SyncSubCommanderContainer(detachment);
    }

    public void SyncStates(Detachment detachment)
    {
        // statesText.text = "";

        var hex = detachment.Hex;
        var ms = detachment.MovingState;

        List<string> texts = new();
        if (ms == null)
        {
            texts.Add($"Idle in ({hex.X},{hex.Y})");
        }
        else
        {
            // detachment.MovingState
            texts.AddRange(new List<string>
            {
                $"Current Speed: {detachment.RealSpeed()}km/day",
                $"Moving from ({detachment.Hex.X},{detachment.Hex.Y}) to ({ms.CurrentTarget.X},{ms.CurrentTarget.Y})",
                $"Completed: {ms.CurrentCompleted.ToString("P")}",
                $"Final Destination: ({ms.FinalTarget.X},{ms.FinalTarget.Y})"
            });
        }
        texts.Add("Ammo: 100%"); // TODO: Add ammo update
        statesText.text = string.Join("\n", texts);
    }

    public void OnCurrentDetachmentMovingStateChanged(Detachment detachment)
    {
        if(gameObject.activeSelf)
            SyncStates(detachment); 
    }

    public void OnDetachmentSelected(Detachment detachment)
    {
        Sync(detachment);
        gameObject.SetActive(true);
    }

    void SyncStrengthContainer(Detachment detachment)
    {
        foreach (Transform t in strengthContainer.transform)
            Destroy(t.gameObject);

        foreach((var category, var strength) in Helpers.GetElementCategoryStrength(detachment.Elements))
        {
            var obj = Instantiate(categoryStrengthItemPrefab, strengthContainer.transform);
            var text = obj.GetComponent<TMP_Text>();
            text.text = $"{category.Name}: {strength}";
        }
    }

    public void OnDetachmentUnselected() => gameObject.SetActive(false);
    public void OnTransferCompleted(Detachment detachment)
    {
        if(detachment.IsEmpty())
        {
            gameObject.SetActive(false);
            return;
        }
        Sync(detachment);
    }

    void SyncSubCommanderContainer(Detachment detachment)
    {
        foreach (Transform t in subCommanderContainer.transform)
            Destroy(t.gameObject);

        var containers = detachment.Elements.Divide(Provider.state.ElementTypeSystem, detachment.Leaders.Count, 0.51f, 1.99f);
        foreach ((var leader, var container) in detachment.Leaders.Zip(containers.Reverse(), (x,y) => (x,y)))
        {
            var obj = Instantiate(commanderPanelPrefab, subCommanderContainer.transform);
            var commanderPanel = obj.GetComponent<CommanderPanel>();
            commanderPanel.Sync(leader, detachment.Side, container);
        }
    }

    public void Close()
    {
        closed.Invoke();
        gameObject.SetActive(false);
    }

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
        detachmentTransferred.Invoke(currentDetachment);
        Debug.Log("OnTransfer");
    }

    public void OnRuleOfEngamentChanged(int idx)
    {
        Debug.Log($"OnRuleOfEngamentChanged {idx}");
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
