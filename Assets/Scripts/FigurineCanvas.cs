using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using System.Linq;

public class FigurineCanvas : MonoBehaviour
{
    public GameManager gameManager;
    public float confrontOffset = 0.2f;
    public GameObject FigurinePrefab;
    public GridLayout grid;

    // Start is called before the first frame update
    void Start()
    {
        Sync(gameManager.state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Sync(GameState state)
    {
        foreach(Transform t in transform)
            Destroy(t.gameObject);

        var hexSideDetachmentsMap = state.GetHexSideDetachmentsMap();

        foreach((var hex, var sideDetachmentsMap) in hexSideDetachmentsMap)
        {
            var center = grid.CellToWorld(new Vector3Int(hex.X, hex.Y, 0));
            if (sideDetachmentsMap.Count == 1)
            {
                CreateFigurine(center, sideDetachmentsMap.Values.First());
            }
            else if(sideDetachmentsMap.Count == 2)
            {
                var offset = new Vector3(confrontOffset, 0, 0);
                CreateFigurine(center + offset, sideDetachmentsMap.Values.First());
                CreateFigurine(center - offset, sideDetachmentsMap.Values.Skip(1).First());
            }
        }
    }

    void CreateFigurine(Vector3 pos, List<Detachment> detachments)
    {
        detachments.Sort((x, y) => -x.GetTotalManpower().CompareTo(y.GetTotalManpower()));
        var detachment = detachments[0];

        var leader = detachment.CurrentLeader;
        var portrait = Resources.Load<Sprite>($"Leaders/{detachment.Side.Name}/{leader.Name}");
        if (portrait == null)
            portrait = Resources.Load<Sprite>($"Flags/{detachment.Side.Name}");

        var text = $"{detachment.GetTotalManpower()}";
        if (!leader.IsPlaceholdLeader)
            text = $"{leader.Name}\n{text}";

        var obj = Instantiate(FigurinePrefab, transform);
        obj.transform.localPosition = pos;
        var figurine = obj.GetComponent<Figurine>();
        figurine.Sync(portrait, text);
    }
}
