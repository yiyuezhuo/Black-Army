using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using UnityEngine.Events;
using System.Linq;

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

    public static string FormatLeaderStats(Leader leader) => $"{leader.Strategic}/{leader.Operational}/{leader.Guerrilla}/{leader.Tactical}";

    public static IEnumerable<(ElementCategory, int)> GetElementCategoryStrength(ElementContainer elementContainer)
    {
        var categoryStrengthMap = Provider.state.ElementCategories.ToDictionary(c => c, c => 0);

        foreach ((var elementType, var elementValue) in elementContainer.Elements)
            categoryStrengthMap[elementType.Category] += elementValue.Strength;

        foreach (var category in Provider.state.ElementCategories)
            yield return (category, categoryStrengthMap[category]);
    }
}

public static class Provider
{
    public static GameState state;
    static Dictionary<(int, int), Hex> hexMap;

    static Provider()
    {
        var data = new YYZ.BlackArmy.Loader.RawData() { reader = new UnityReader() };
        data.Load();

        Debug.Log(data);

        state = data.GetGameState();
        hexMap = state.Hexes.ToDictionary(hex => (hex.X, hex.Y));

        Debug.Log(state);
    }

    public static Hex GetHex(int x, int y) => hexMap[(x, y)];
    public static Hex GetHex((int, int) xy) => hexMap[xy];

    static UnityEvent testEvent;
}