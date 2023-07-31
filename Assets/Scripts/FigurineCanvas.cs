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
        Sync(Provider.state);
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
                (var side, var detachments) = sideDetachmentsMap.First();
                CreateFigurine(center, hex, side, detachments);
            }
            else if(sideDetachmentsMap.Count == 2)
            {
                var offset = new Vector3(confrontOffset, 0, 0);
                (var side1, var detachments1) = sideDetachmentsMap.First();
                (var side2, var detachments2) = sideDetachmentsMap.Skip(1).First();
                CreateFigurine(center + offset, hex, side1, detachments1);
                CreateFigurine(center - offset, hex, side2, detachments2);
            }
        }
    }

    void CreateFigurine(Vector3 pos, Hex hex, Side side, List<Detachment> detachments)
    {
        detachments.Sort((x, y) => -x.GetTotalManpower().CompareTo(y.GetTotalManpower()));
        var detachment = detachments[0];

        var leader = detachment.CurrentLeader;
        /*
        var portrait = Resources.Load<Sprite>($"Leaders/{detachment.Side.Name}/{leader.Name}");
        if (portrait == null)
            portrait = Resources.Load<Sprite>($"Flags/{detachment.Side.Name}");
        */
        var portrait = Helpers.GetSprite(leader, side);

        var text = $"{detachment.GetTotalManpower()}";
        if (!leader.IsPlaceholdLeader)
            text = $"{leader.Name}\n{text}";

        var obj = Instantiate(FigurinePrefab, transform);
        obj.transform.localPosition = pos;
        var figurine = obj.GetComponent<Figurine>();
        figurine.hex = hex;
        figurine.side = side;
        figurine.clicked.AddListener(gameManager.stackClicked);

        figurine.Sync(portrait, text);
    }
}
