using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using System.Linq;
using System.IO;

public class UnityReader: YYZ.BlackArmy.Loader.ITableReader
{
    public byte[] Read(string name)
    {
        var path = "TableData/" + Path.GetFileNameWithoutExtension(name);
        var textAsset = Resources.Load<TextAsset>(path);
        return textAsset.bytes;
    }
}

public class CounterCanvas : MonoBehaviour
{
    public float confrontOffset = 0.2f;
    // public GameState state;
    public GameObject Counter2DPrefab;
    public GridLayout grid;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        Sync(gameManager.state);
    }

    public void Sync(GameState state)
    {
        foreach(Transform t in transform)
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
                CreateSprite(center + new Vector3(0, confrontOffset, 0), side, strength); // Evade location label
            }
            else if(sideStrengthMap.Count == 2)
            {
                (var side, var strength) = sideStrengthMap.First();
                CreateSprite(center + new Vector3(0, confrontOffset, 0), side, strength);

                (side, strength) = sideStrengthMap.Skip(1).First();
                CreateSprite(center - new Vector3(0, confrontOffset, 0), side, strength);
            }
        }
    }

    void CreateSprite(Vector3 pos, Side side, int strength)
    {
        var obj = Instantiate(Counter2DPrefab, transform);
        obj.transform.localPosition = pos;
        var counter = obj.GetComponent<Counter2D>();
        var sprite = Resources.Load<Sprite>("Flags/" + side.Name);
        counter.Sync(strength.ToString(), sprite);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
