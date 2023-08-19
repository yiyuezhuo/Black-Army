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

    // VisualElement basePanel;

    // VisualElement root;

    Label LocationUI;
    Label DateUI;

    SymmetryUI LeftUI;
    SymmetryUI RightUI;

    public ListView SubCombatListView;

    Button ConfirmButton;

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
            var leaderStats = root.Q(prefix + "LeaderStats") as Label;
            leaderStats.AddManipulator(new TooltipManipulator());

            var subCombatResults = root.Q(prefix + "SubCombatResults") as Label;
            subCombatResults.AddManipulator(new TooltipManipulator());

            return new()
            {
                LeaderPortrait = root.Q(prefix + "LeaderPortrait"),
                TextSummary = root.Q(prefix + "TextSummary") as Label,
                LeaderStats = leaderStats,
                SubCombatTypes = root.Q(prefix + "SubCombatTypes") as Label,
                SubCombatResults = subCombatResults
            };
        }
    }

    public void SetVisualElement(VisualElement root)
    {
        // root = doc.rootVisualElement;

        LocationUI = root.Q("Location") as Label;
        DateUI = root.Q("Date") as Label;
        SubCombatListView = root.Q("SubCombatListView") as ListView;

        LeftUI = SymmetryUI.Generate(root, "Left");
        RightUI = SymmetryUI.Generate(root, "Right");

        BindSubCombatListView();

        ConfirmButton = root.Q("ConfirmButton") as Button;
        ConfirmButton.RegisterCallback<ClickEvent>((evt) => root.RemoveFromHierarchy());

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

        // SubCombatDataList.Clear();
        // SubCombatDataList.AddRange(messageList.Select(message => EncodeSubCombatData(resolver, message)));
        SubCombatListView.itemsSource = SubCombatDataList = messageList.Select(message => EncodeSubCombatData(resolver, message)).ToList();
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
            CombatTypeSprite=Helpers.GetSprite(message.Result.ResultSummary),
            CombatTypeTooltip = message.Result.ResultSummary.Name
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
        
        var lost = 0;
        var committed = 0;
        // var situation = group.Situation;
        var situationDelta = 0f;

        foreach (var message in messages)
        {
            var sideMessage = isAttacker ? message.Attacker : message.Defender;
            situationDelta += sideMessage.SituationDelta;

            foreach (var unit in sideMessage.UnitsCommitted)
            {
                lost += unit.Lost;
                committed += unit.Committed;
            }
            // message.Combat.Type;
            // message.Result;
        }

        var total = group.Units.Sum(u => u.Strength) + lost;

        var relatedMessages = messages.Where(m => (m.Combat.AttackerInitiative && isAttacker) || (!m.Combat.AttackerInitiative && !isAttacker)).ToList();
        var typeCounterS = string.Join(", ", relatedMessages.GroupBy(m => m.Combat.Type).Select(g => $"{g.Key}:{g.Count()}"));
        var resultSummaryCounterS = string.Join(", ", relatedMessages.GroupBy(m => m.Result.ResultSummary).Select(g=>$"{g.Key.ShortName}:{g.Count()}"));

        var totalTactic = 0; // TODO: Add tactic modifier
        var situation = group.Situation - situationDelta;
        var moveSpeed = 0; // TODO: Add Move Speed Modifier

        string chanceS;

        if (messages.Count == 0)
            chanceS = "-/-";
        else
        {
            var combatSide0 = isAttacker ? messages[0].Combat.Attacker : messages[0].Combat.Defender;
            // var p1 = combatSide0.BeginChance.Potential.ToString("0.#");
            // var p2 = combatSide0.BeginChance.Baseline.ToString("0.#");
            var p1 = combatSide0.BeginChance.Potential.ToString("N0");
            var p2 = combatSide0.BeginChance.Baseline.ToString("N0");
            chanceS = $"{p1}/{p2}";
        }

        ui.TextSummary.text = @$"{total} men (-{lost})
Committed: {committed} pts
Total Tactic: {totalTactic} (0%)
Situation: {situation.ToString("P2")} ({situationDelta.ToString("P2")})
Move Speed: {moveSpeed.ToString("P2")}
Chance: {chanceS}";

        ui.SubCombatTypes.text = typeCounterS;
        ui.SubCombatResults.text = resultSummaryCounterS;
    }
}
