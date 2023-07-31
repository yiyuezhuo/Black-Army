using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using System.Linq;
using TMPro;
using UnityEngine.Events;

public class TransferDialog : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Transform transferRows;
    public Transform commanderToggleContainer;

    public GameObject transferRowPrefab;
    public GameObject commanderTogglePrefab;

    // public GameManager gameManager;

    public UnityEvent detachmentsChanged;

    Detachment currentDetachment;
    Detachment dummy;
    List<Detachment> detachmentOptions;
    
    List<ElementType> elementTypes;

    public void Sync(Detachment detachment)
    {
        currentDetachment = detachment;
        dummy = new Detachment();
        detachmentOptions = new List<Detachment>() { dummy };
        detachmentOptions.AddRange(detachment.Hex.Detachments.Where(d => d.Side == detachment.Side && d != detachment));

        var options = detachmentOptions.Select(d => d == dummy ? "(Create A New Detachment)" : d.Name).ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        // var dummy = new Detachment();
        Sync(detachment, dummy);
    }

    public void OnDetachmentTransfrred(Detachment d)
    {
        gameObject.SetActive(true);
        Sync(d);
    }

    public void Sync(Detachment src, Detachment dst)
    {
        elementTypes = new();
        foreach (Transform t in transferRows)
            Destroy(t.gameObject);

        foreach ((var elementType, var elementValue) in src.Elements.Elements)
        {
            elementTypes.Add(elementType);
            var obj = Instantiate(transferRowPrefab, transferRows);
            var transferRow = obj.GetComponent<TransferRow>();
            transferRow.Sync(elementType.Name, elementValue.Strength, dst.Elements.StrengthOf(elementType));
        }

        foreach (Transform t in commanderToggleContainer)
            Destroy(t.gameObject);

        foreach (var leader in src.Leaders)
        {
            var obj = Instantiate(commanderTogglePrefab, commanderToggleContainer);
            obj.GetComponent<CommanderToggle>().Sync($"{leader.Name}{Helpers.FormatLeaderStats(leader)}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDropdownValueChanged(int idx)
    {
        Debug.Log($"OnDropdownValueChanged: {idx}");
        Sync(currentDetachment, detachmentOptions[idx]);
    }

    public void OnConfirm()
    {
        Debug.Log("OnConfirm");
        gameObject.SetActive(false);

        var dst = detachmentOptions[dropdown.value];

        var idx = 0;
        foreach(var row in transferRows.GetComponentsInChildren<TransferRow>())
        {
            var elementType = elementTypes[idx];

            currentDetachment.Elements.TransferTo(dst.Elements, elementType, (int)row.slider.value);

            idx++;
        }

        idx = 0;
        var transferredLeaders = new List<Leader>();
        foreach(var toggle in commanderToggleContainer.GetComponentsInChildren<UnityEngine.UI.Toggle>())
        {
            if(toggle.isOn)
            {
                transferredLeaders.Add(currentDetachment.Leaders[idx]);
            }
            idx++;
        }

        foreach(var leader in transferredLeaders)
        {
            currentDetachment.TransferTo(dst, leader);
        }

        var changed = false;

        if (dst == dummy)
        {
            // Create A New Detachment actually.
            dst.Name = dst.Leaders.Count == 0 ? "New Detachment" : $"{dst.CurrentLeader.Name}'s Detachment";

            dst.Side = currentDetachment.Side;
            currentDetachment.Side.Detachments.Add(dst); // TODO: Refactor
            dst.Hex = currentDetachment.Hex;
            currentDetachment.Hex.Detachments.Add(dst); // TODO: Refactor

            changed = true;
        }

        if(currentDetachment.IsEmpty())
        {
            currentDetachment.Side.Detachments.Remove(currentDetachment); // TODO: Refactor
            currentDetachment.Hex.Detachments.Remove(currentDetachment); // TODO:
                                                                         
            changed = true;
        }

        if(changed)
            detachmentsChanged.Invoke();
    }

    public void OnCancel()
    {
        Debug.Log("OnCancel");
        gameObject.SetActive(false);
    }

    public void OnTransferAll()
    {
        Debug.Log("OnTransferAll");

        foreach (var row in transferRows.GetComponentsInChildren<TransferRow>())
        {
            row.slider.value = row.slider.maxValue;
        }

        foreach(var toggle in commanderToggleContainer.GetComponentsInChildren<UnityEngine.UI.Toggle>())
        {
            toggle.isOn = true;
        }

        // gameObject.SetActive(false);
    }
}
