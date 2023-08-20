using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using YYZ.BlackArmy.Model;

public class CampaignUIDocumentController : MonoBehaviour
{
    UIDocument doc;
    SubTurnResolverBarController subTurnResolveProgressionController;

    VisualElement subTurnResolveProgression;

    int subTurnIdx = 0;


    private void Awake()
    {
        doc = GetComponent<UIDocument>();

        subTurnResolveProgression = doc.rootVisualElement.Q("SubTurnResolveProgression");
        subTurnResolveProgressionController = new();
        subTurnResolveProgressionController.SetVisualElement(subTurnResolveProgression);

        subTurnResolveProgression.style.display = DisplayStyle.None;

        // doc.rootVisualElement.Q
    }

    public void OnTurnResolveBegun()
    {
        subTurnResolveProgression.style.display = DisplayStyle.Flex;
        subTurnIdx = 0;
    }

    public void OnTurnResolveStepped()
    {
        subTurnIdx += 1;
        subTurnResolveProgressionController.SetData(subTurnIdx, GameParameters.SubTurns);
    }

    public void OnTurnResolveCompleted()
    {
        subTurnResolveProgression.style.display = DisplayStyle.None;
    }

    /*
    public class Data
    {
        public int CurrentSubTurn;
        public int TotalSubTurn;
    }

    public void SetData(Data d)
    {
        subTurnResolveProgressionController.SetData(d.CurrentSubTurn, d.TotalSubTurn);
    }
    */

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
