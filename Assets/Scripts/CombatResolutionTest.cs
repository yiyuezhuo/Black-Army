using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YYZ.BlackArmy.Model;
using System.Linq;
using System;
using YYZ.BlackArmy.CombatResolution;
using YYZ.BlackArmy.Loader;
using UnityEngine.UIElements;

public class CombatResolutionTest : MonoBehaviour
{
    public int FixedItemHeight = 54;
    public VisualTreeAsset SubCombatLisyEntryTemplate;
    public VisualTreeAsset CombatResolutionTemplate;

    public bool SingleMode = true;

    // Start is called before the first frame update
    void Start()
    {
        var gameState = Provider.state;

        var hexesEngaged = gameState.Hexes.Where(IsEngage);

        /*
        foreach(var hex in hexesEngaged.Take(2))
        {
            Debug.Log($"hex={hex}");
            Create(hex);
        }
        */
        
        if(SingleMode)
        {
            var hexMaxEngaged = MaxBy(hexesEngaged, hex =>
                hex.Detachments.GroupBy(d => d.Side).Select(g =>
                    g.Sum(d => d.GetTotalManpower())
                ).Min()
            );
            Debug.Log($"hexMaxEngaged={hexMaxEngaged}");

            Create(hexMaxEngaged);
        }
        else
        {
            // foreach (var hex in hexesEngaged.Take(2))
            foreach (var hex in hexesEngaged)
            // foreach (var hex in hexesEngaged.Skip(2))
            {
                Debug.Log($"hex={hex}");
                Create(hex);
            }
        }

    }

    void Create(Hex hex)
    {
        // Debug.Log($"hex={hex}");

        var resolver = new Resolver(hex);
        var messages = resolver.Resolve().ToList();
        /*
        foreach (var message in messages)
        {
            Debug.Log(message);
        }
        */

        var controller = new CombatResolutionController()
        {
            SubCombatLisyEntryTemplate = SubCombatLisyEntryTemplate,
            FixedItemHeight = FixedItemHeight
        };

        var doc = GetComponent<UIDocument>();
        VisualElement element;
        if(SingleMode)
        {
            element = doc.rootVisualElement;
        }
        else
        {
            element = CombatResolutionTemplate.Instantiate();
            doc.rootVisualElement.Add(element);
        }
        controller.SetVisualElement(element);
        controller.Sync(resolver, messages);
    }

    // TODO: Merge implementation
    static T MaxBy<T>(IEnumerable<T> collection, Func<T, int> f)
    {
        var max = int.MinValue;
        var maxEl = default(T);
        foreach (var el in collection)
        {
            var x = f(el);
            if (x > max)
            {
                max = x;
                maxEl = el;
            }
        }
        return maxEl;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    bool IsEngage(Hex hex) => hex.Detachments.GroupBy(d => d.Side).Count() > 1;

}
