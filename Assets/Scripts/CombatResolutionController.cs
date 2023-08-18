using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using YYZ.BlackArmy.CombatResolution;
using static YYZ.BlackArmy.CombatResolution.Resolver;

public class CombatResolutionController
{
    public int FixedItemHeight;
    public VisualTreeAsset SubCombatLisyEntryTemplate;

    Label LocationUI;
    Label DateUI;

    SymmetryUI LeftUI;
    SymmetryUI RightUI;

    public ListView SubCombatListView;

    public List<SubCombatListEntryController.Data> SubCombatDataList = new();

    class SymmetryUI
    {
        public VisualElement LeaderPortrait;
        public Label TextSummary;
        public Label LeaderStats;
        public Label SubCombatTypes;
        public Label SubCombatResults;
        public static SymmetryUI Generate(VisualElement root, string prefix)
        {
            return new()
            {
                LeaderPortrait = root.Q(prefix + "LeaderPortrait"),
                TextSummary = root.Q(prefix + "TextSummary") as Label,
                LeaderStats = root.Q(prefix + "LeaderStats") as Label,
                SubCombatTypes = root.Q(prefix + "SubCombatTypes") as Label,
                SubCombatResults = root.Q(prefix + "SubCombatResults") as Label
            };
        }
    }

    public void Bind(UIDocument doc)
    {
        var root = doc.rootVisualElement;

        LocationUI = root.Q("Location") as Label;
        DateUI = root.Q("Date") as Label;
        SubCombatListView = root.Q("SubCombatListView") as ListView;

        LeftUI = SymmetryUI.Generate(root, "Left");
        RightUI = SymmetryUI.Generate(root, "Right");

        BindSubCombatListView();

        root.AddManipulator(new SimpleDraggingManipulator());
    }

    void BindSubCombatListView()
    {
        SubCombatListView.makeItem = () =>
        {
            var newListEntry = SubCombatLisyEntryTemplate.Instantiate();
            var newListEntryController = new SubCombatListEntryController();
            newListEntry.userData = newListEntryController;
            newListEntryController.SetVisualElement(newListEntry);
            return newListEntry;
        };

        SubCombatListView.bindItem = (item, index) =>
        {
            (item.userData as SubCombatListEntryController).SetData(SubCombatDataList[index]);
        };

        SubCombatListView.fixedItemHeight = FixedItemHeight;
        SubCombatListView.itemsSource = SubCombatDataList;
    }

    public void Sync(YYZ.BlackArmy.CombatResolution.Resolver resolver, IEnumerable<ResolveMessage> messages)
    {
        var messageList = messages.ToList();

        var gameState = Provider.state;
        var hex = resolver.Hex;
        var name = $"({hex.X},{hex.Y})";
        LocationUI.text = $"The Combat in {name}";
        DateUI.text = gameState.CurrentDateTime.ToString("dddd, dd MMMM yyyy"); // TODO: sub turn handling
                                                                                // resolver.AttackerGroup.Leader
                                                                                // resolver.AttackerGroup.Leader.Name;
        var attackerGroup = resolver.AttackerGroup;
        var defenderGroup = resolver.DefenderGroup;

        SyncCombatGroup(LeftUI, attackerGroup, messageList, true);
        SyncCombatGroup(RightUI, defenderGroup, messageList, false);

        SubCombatDataList.Clear();
        SubCombatDataList.AddRange(messageList.Select(message => EncodeSubCombatData(resolver, message)));
        // var newSubCombatDataList = messages.
    }

    SubCombatListEntryController.Data EncodeSubCombatData(YYZ.BlackArmy.CombatResolution.Resolver resolver, ResolveMessage message)
    {
        var attacker = EncodeSubCombatDataEntry(resolver, message, true);
        var defender = EncodeSubCombatDataEntry(resolver, message, false);
        (var left, var right) = message.Combat.AttackerInitiative ? (attacker, defender) : (defender, attacker);
        return new()
        {
            Left = left,
            Right = right,
            Tint = message.Combat.Type == YYZ.CombatGenerator.CombatType.Fire ? new Color(255, 255, 0) : new Color(255, 0, 0),
            CombatTypeSprite=Helpers.GetSprite(message.Result.ResultSummary)
        };
    }

    SubCombatListEntryController.SideData EncodeSubCombatDataEntry(YYZ.BlackArmy.CombatResolution.Resolver resolver, ResolveMessage message, bool isSubCombatAttacker)
    {
        var group = isSubCombatAttacker ? resolver.AttackerGroup : resolver.DefenderGroup;
        var sideMessage = isSubCombatAttacker ? message.Attacker : message.Defender;
        var combatMessage = isSubCombatAttacker ? message.Combat.Attacker : message.Combat.Defender;

        var potentialDelta = combatMessage.EndChance.Potential - combatMessage.BeginChance.Potential;
        var baselineDelta = combatMessage.EndChance.Baseline - combatMessage.BeginChance.Baseline;

        return new()
        {
            Flag=Helpers.GetSprite(group.Side),
            Committed = sideMessage.UnitsCommitted.Sum(unit => unit.Committed),
            CommittedLost= sideMessage.UnitsCommitted.Sum(unit => unit.Lost),
            SituationDelta= sideMessage.SituationDelta,
            ChancePotentialDelta = potentialDelta,
            ChanceBaselineDelta= baselineDelta,
        };
    }

    void SyncCombatGroup(SymmetryUI ui, CombatGroup group, List<ResolveMessage> messages, bool isAttacker)
    {
        var p = Helpers.GetSprite(group.Leader, group.Side);
        ui.LeaderPortrait.style.backgroundImage = new StyleBackground(p);
        var leaderStats = Helpers.FormatLeaderStats(group.Leader);
        ui.LeaderStats.text = $"{group.Leader.Name}({leaderStats})";

        var total = group.Units.Sum(u => u.Strength);
        var lost = 0;
        var committed = 0;
        // var situation = group.Situation;
        var situationDelta = 0f;

        var combatSide0 = isAttacker ? messages[0].Combat.Attacker : messages[0].Combat.Defender;
        foreach (var message in messages)
        {
            var sideMessage = isAttacker ? message.Attacker : message.Defender;
            foreach(var unit in sideMessage.UnitsCommitted)
            {
                lost += unit.Lost;
                committed += unit.Committed;
                situationDelta += sideMessage.SituationDelta;
            }

            // message.Combat.Type;
            // message.Result;
        }

        var relatedMessages = messages.Where(m => (m.Combat.AttackerInitiative && isAttacker) || (!m.Combat.AttackerInitiative && !isAttacker)).ToList();
        var typeCounterS = string.Join(", ", relatedMessages.GroupBy(m => m.Combat.Type).Select(g => $"{g.Key}:{g.Count()}"));
        var resultSummaryCounterS = string.Join(", ", relatedMessages.GroupBy(m => m.Result.ResultSummary).Select(g=>$"{g.Key.ShortName}:{g.Count()}"));

        var totalTactic = 0; // TODO: Add tactic modifier
        var situation = group.Situation - situationDelta;
        var moveSpeed = 0; // TODO: Add Move Speed Modifier
        ui.TextSummary.text = @$"{total} men (-{lost})
Committed: {committed} pts
Total Tactic: {totalTactic} (0%)
Situation: {situation} ({situationDelta})
Move Speed: {moveSpeed}
Chance: {combatSide0.BeginChance.Potential}/{combatSide0.BeginChance.Baseline}";

        ui.SubCombatTypes.text = typeCounterS;
        ui.SubCombatResults.text = resultSummaryCounterS;
    }
}
