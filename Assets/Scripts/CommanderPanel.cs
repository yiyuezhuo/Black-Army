using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YYZ.BlackArmy.Model;
using System.Linq;

public class CommanderPanel : MonoBehaviour
{
    public UnityEngine.UI.Image portrait;
    public TMP_Text commanderNameText;
    public TMP_Text commanderStatsText;
    public TMP_Text commandingStrengthText;

    public void Sync(Leader leader, Side side, ElementContainer container)
    {
        portrait.sprite = Helpers.GetSprite(leader, side);

        commanderNameText.text = leader.Name;
        commanderStatsText.text = Helpers.FormatLeaderStats(leader);

        var csList = Helpers.GetElementCategoryStrength(container).Where(cs => cs.Item2 > 0).ToList();
        if (csList.Count == 0)
            commandingStrengthText.text = "Commanding No Units";
        else
        {
            var s = string.Join(",", csList.Select(cs => $"{cs.Item2} {cs.Item1.Name}"));
            commandingStrengthText.text = $"Commanding {s}";
        }

        portrait.GetComponent<SubLeaderImage>().leader = leader;
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
