using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YYZ.BlackArmy.Model;
using System.Linq;

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

    public void Sync(Detachment detachment)
    {
        gameObject.SetActive(true);

        // Debug.Log($"UnitBar sync: {detachment}");

        detachmentNameText.text = detachment.Name;
        detachmentLeaderImage.sprite = Helpers.GetSprite(detachment);

        SyncStrengthContainer(detachment);

        var leader = detachment.CurrentLeader;
        commanderNameText.text = leader.Name;
        commanderStrategicText.text = $"Strategic: {leader.Strategic}";
        commanderOperationalText.text = $"Operational: {leader.Operational}";
        commanderGuerrillaText.text = $"Guerrilla: {leader.Guerrilla}";
        commanderTacticalText.text = $"Tactical: {leader.Tactical}";

        statesText.text = "";

        var tacticalSum = detachment.Leaders.Sum(l => l.Tactical);
        commandersStatsText.text = $"{detachment.Leaders.Count} Commanders, tactical sum:{tacticalSum}, modifier:+0%";

        SyncSubCommanderContainer(detachment);
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

        /*
        var categoryStrengthMap = Provider.state.ElementCategories.ToDictionary(c => c, c => 0);

        foreach ((var elementType, var elementValue) in detachment.Elements.Elements)
        {
            categoryStrengthMap[elementType.Category] += elementValue.Strength;
        }

        foreach (var category in Provider.state.ElementCategories)
        {
            var obj = Instantiate(categoryStrengthItemPrefab, strengthContainer.transform);
            var text = obj.GetComponent<TMP_Text>();
            text.text = $"{category.Name}:{categoryStrengthMap[category]}";
        }
        */

        foreach((var category, var strength) in Helpers.GetElementCategoryStrength(detachment.Elements))
        {
            var obj = Instantiate(categoryStrengthItemPrefab, strengthContainer.transform);
            var text = obj.GetComponent<TMP_Text>();
            text.text = $"{category.Name}: {strength}";
        }
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
