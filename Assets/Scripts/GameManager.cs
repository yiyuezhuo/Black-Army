using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;

public class GameManager : MonoBehaviour
{
    public GameState state;

    private void Awake()
    {
        var data = new YYZ.BlackArmy.Loader.RawData() { reader = new UnityReader() };
        data.Load();
        state = data.GetGameState();
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
