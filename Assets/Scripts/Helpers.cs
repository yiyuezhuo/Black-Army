using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;

public static class Helpers
{
    public static Sprite GetSprite(Side side)
    {
        return Resources.Load<Sprite>("Flags/" + side.Name);
    }

    public static Sprite GetSprite(Detachment detachment)
    {
        return GetSprite(detachment.CurrentLeader, detachment.Side);
    }

    public static Sprite GetSprite(Leader leader, Side side)
    {
        var portrait = Resources.Load<Sprite>($"Leaders/{side.Name}/{leader.Name}");
        if (portrait == null)
            portrait = Resources.Load<Sprite>($"Flags/{side.Name}");

        return portrait;
    }
}

public static class Provider
{
    public static GameState state;
    static Provider()
    {
        var data = new YYZ.BlackArmy.Loader.RawData() { reader = new UnityReader() };
        data.Load();
        state = data.GetGameState();

        Debug.Log(state);
    }
}