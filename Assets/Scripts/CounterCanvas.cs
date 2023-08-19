using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using System.Linq;
using System;
// using System.IO;


public class CounterCanvas : MonoBehaviour
{
    public float confrontOffset = 0.2f;
    // public GameState state;
    public GameObject Counter2DPrefab;
    public GridLayout grid;
    public GameManager gameManager;

    // public Func<Hex, Side> ff;

    // Start is called before the first frame update
    void Start()
    {
        Sync();
    }

    public void Sync()
    {
        var state = Provider.state;

        foreach (Transform t in transform)
        {
            Destroy(t.gameObject); // TODO: Object Pooling?
        }

        var hexSideStrengthMap = state.GetHexSideStrengthMap();

        foreach((var hex, var sideStrengthMap) in hexSideStrengthMap)
        {
            var center = grid.CellToWorld(new Vector3Int(hex.X, hex.Y, 0));
            if (sideStrengthMap.Count == 1)
            {
                (var side, var strength) = sideStrengthMap.First();
                // CreateSprite(center, side, strength);
                CreateSprite(center + new Vector3(0, confrontOffset, 0), hex, side, strength); // Evade location label
            }
            else if(sideStrengthMap.Count == 2)
            {
                (var side, var strength) = sideStrengthMap.First();
                CreateSprite(center + new Vector3(0, confrontOffset, 0), hex, side, strength);

                (side, strength) = sideStrengthMap.Skip(1).First();
                CreateSprite(center - new Vector3(0, confrontOffset, 0), hex, side, strength);
            }
        }
    }

    public void OnDetachmentsChanged()
    {
        if(gameObject.activeSelf)
            Sync();
    }

    public void OnNewTurnArrived() => OnDetachmentsChanged();

    void CreateSprite(Vector3 pos, Hex hex, Side side, int strength)
    {
        var obj = Instantiate(Counter2DPrefab, transform);
        obj.transform.localPosition = pos;

        var counter = obj.GetComponent<Counter2D>();
        counter.side = side;
        counter.hex = hex;
        counter.clicked.AddListener(gameManager.stackClicked); // TODO: decouple GameManager

        // var sprite = Resources.Load<Sprite>("Flags/" + side.Name);
        var sprite = Helpers.GetSprite(side);
        counter.Sync(strength.ToString(), sprite);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
