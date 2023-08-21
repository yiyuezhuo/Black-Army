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
    public VisualTreeAsset StrengthStatsRowTemplate;

    public Mode TestMode;

    public enum Mode
    {
        Single,
        Multiple,
        Event
    }

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
        
        switch(TestMode)
        {
            case Mode.Single:
                var hexMaxEngaged = MaxBy(hexesEngaged, hex =>
                    hex.Detachments.GroupBy(d => d.Side).Select(g =>
                        g.Sum(d => d.GetTotalManpower())
                    ).Min()
                );
                Debug.Log($"hexMaxEngaged={hexMaxEngaged}");

                Create(hexMaxEngaged);
                break;
            case Mode.Multiple:
                // foreach (var hex in hexesEngaged.Take(2))
                foreach (var hex in hexesEngaged)
                // foreach (var hex in hexesEngaged.Skip(2))
                {
                    Debug.Log($"hex={hex}");
                    Create(hex);
                }
                break;
            case Mode.Event:
                break;
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

        Create(resolver, messages);

    }

    void Create(Resolver resolver, List<Resolver.ResolveMessage> messages)
    {
        var controller = new CombatResolutionController()
        {
            SubCombatLisyEntryTemplate = SubCombatLisyEntryTemplate,
            // StrengthStatsRowTemplate = StrengthStatsRowTemplate,
            FixedItemHeight = FixedItemHeight
        };

        var doc = GetComponent<UIDocument>();
        VisualElement element;
        if (TestMode == Mode.Single)
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

        element.AddManipulator(new SimpleDraggingManipulator());
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
