using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using YYZ.BlackArmy.CombatResolution;
using YYZ.CombatGenerator;
using YYZ.CombatResolution;
using static CombatResolutionController.SnapshotData;
using static YYZ.BlackArmy.CombatResolution.Resolver;
using YYZ.BlackArmy.Model;

public class CombatResolutionController
{
    public int FixedItemHeight;
    public VisualTreeAsset SubCombatLisyEntryTemplate;
    // public VisualTreeAsset StrengthStatsRowTemplate;

    // VisualElement basePanel;

    // VisualElement root;

    Label LocationUI;
    Label DateUI;

    SymmetryUI LeftUI;
    SymmetryUI RightUI;

    VisualElement CategorySummaryGrid;

    public ListView SubCombatListView;

    public Button ConfirmButton;

    public List<SubCombatListEntryController.Data> SubCombatDataList = new();

    public float CategoryStatsCellWidth = 10f; // 10% * 8
    public float CategoryStatsLabelWidth = 19f;

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

        CategorySummaryGrid = root.Q("CategorySummaryGrid");

        BindSubCombatListView();

        ConfirmButton = root.Q("ConfirmButton") as Button;
        ConfirmButton.RegisterCallback<ClickEvent>((evt) => root.RemoveFromHierarchy());

        // root.AddManipulator(new SimpleDraggingManipulator());
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
        var data = new SnapshotData(resolver, messages);
        Sync(data);
    }

    public void Sync(SnapshotData data)
    {
        LocationUI.text = $"The Combat in {data.LocationName}";
        DateUI.text = data.Date.ToString("dddd, dd MMMM yyyy");

        SyncCombatGroup(LeftUI, data.AttackerData);
        SyncCombatGroup(RightUI, data.DefenderData);

        CategorySummaryGrid.Clear();
        foreach (var category in Provider.state.ElementCategories)
        {
            var aHas = data.AttackerData.CategoryStatsMap.ContainsKey(category);
            var dHas = data.DefenderData.CategoryStatsMap.ContainsKey(category);
            if(!aHas && !dHas)
                continue;
            if (!data.AttackerData.CategoryStatsMap.TryGetValue(category, out var aVal))
                aVal = new();
            if (!data.DefenderData.CategoryStatsMap.TryGetValue(category, out var dVal))
                dVal = new();

            CreateLabelsFor(aVal);
            CreateLabel(category.Name);
            CreateLabelsFor(dVal);


            /*
            var row = StrengthStatsRowTemplate.Instantiate();

            SetStrengthValue(row.Q("StrengthStatsLeft"), aVal);
            SetStrengthValue(row.Q("StrengthStatsRight"), dVal);

            CategorySummaryGrid.Add(row);
            */
        }

        SubCombatListView.itemsSource = SubCombatDataList = data.SubCombatDataList;
    }

    void CreateLabelsFor(CombatGroupData.StrengthStats val)
    {
        CreateCell(val.Total);
        CreateCell(val.Committed);
        CreateCell(val.Lost);
        CreateCell(val.Remain);
    }

    public void CreateCell(int num)
    {
        var label = CreateLabel();
        label.style.width = Length.Percent(CategoryStatsCellWidth);
        label.text = num.ToString();
        CategorySummaryGrid.Add(label);
    }

    public void CreateLabel(string name)
    {
        var label = CreateLabel();
        label.style.width = Length.Percent(CategoryStatsLabelWidth);
        label.text = name;
        CategorySummaryGrid.Add(label);
    }

    public Label CreateLabel()
    {
        var label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        return label;
    }

    /*
    void SetStrengthValue(VisualElement el, CombatGroupData.StrengthStats val)
    {
        (el.Q("Total") as Label).text = val.Total.ToString();
        (el.Q("Commit") as Label).text = val.Committed.ToString();
        (el.Q("Lost") as Label).text = val.Lost.ToString();
        (el.Q("Remain") as Label).text = val.Remain.ToString();
    }
    */

    public class SnapshotData
    {
        public string LocationName;
        public DateTime Date;

        public CombatGroupData AttackerData;
        public CombatGroupData DefenderData;

        public List<SubCombatListEntryController.Data> SubCombatDataList;

        public SnapshotData(YYZ.BlackArmy.CombatResolution.Resolver resolver, IEnumerable<ResolveMessage> messages)
        {
            var hex = resolver.Hex;
            var gameState = Provider.state;
            var messageList = messages.ToList();

            LocationName = $"({hex.X},{hex.Y})";
            Date = gameState.CurrentDateTime;

            AttackerData = CombatGroupData.Create(resolver.AttackerGroup, messageList, true);
            DefenderData = CombatGroupData.Create(resolver.DefenderGroup, messageList, false);

            SubCombatDataList = messageList.Select(message => EncodeSubCombatData(resolver, message)).ToList();
        }

        static SubCombatListEntryController.Data EncodeSubCombatData(YYZ.BlackArmy.CombatResolution.Resolver resolver, ResolveMessage message)
        {
            var attacker = EncodeSubCombatDataEntry(resolver, message, true);
            var defender = EncodeSubCombatDataEntry(resolver, message, false);
            (var left, var right) = message.Combat.AttackerInitiative ? (attacker, defender) : (defender, attacker);
            return new()
            {
                Left = left,
                Right = right,
                Tint = message.Combat.Type == YYZ.CombatGenerator.CombatType.Fire ? new Color(255, 255, 0) : new Color(255, 0, 0),
                CombatTypeSprite = Helpers.GetSprite(message.Result.ResultSummary),
                CombatTypeTooltip = message.Result.ResultSummary.Name
            };
        }

        static SubCombatListEntryController.SideData EncodeSubCombatDataEntry(YYZ.BlackArmy.CombatResolution.Resolver resolver, ResolveMessage message, bool isSubCombatAttacker)
        {
            var group = isSubCombatAttacker ? resolver.AttackerGroup : resolver.DefenderGroup;
            var sideMessage = isSubCombatAttacker ? message.Attacker : message.Defender;
            var combatMessage = isSubCombatAttacker ? message.Combat.Attacker : message.Combat.Defender;

            var potentialDelta = combatMessage.EndChance.Potential - combatMessage.BeginChance.Potential;
            var baselineDelta = combatMessage.EndChance.Baseline - combatMessage.BeginChance.Baseline;

            return new()
            {
                Flag = Helpers.GetSprite(group.Side),
                Committed = sideMessage.UnitsCommitted.Sum(unit => unit.Committed),
                CommittedLost = sideMessage.UnitsCommitted.Sum(unit => unit.Lost),
                SituationDelta = sideMessage.SituationDelta,
                ChancePotentialDelta = potentialDelta,
                ChanceBaselineDelta = baselineDelta,
            };
        }


        public class CombatGroupData
        {
            public Sprite LeaderPortrait;
            public string LeaderDescription;
            public int Total;
            public int Lost;
            public int Committed;
            public float TotalTactic;
            public float Situation;
            public float SituationDelta;
            public float MoveSpeedModifier;
            public float BeginPotentialChance;
            public float BeginBaselineChance;
            public (CombatType, int)[] TypeCounter;
            public (CombatResultSummary, int)[] ResultSummaryCounter;
            public Dictionary<ElementCategory, StrengthStats> CategoryStatsMap;

            public class StrengthStats
            {
                public int Lost;
                public int Committed;
                public int Total;
                public int Remain { get => Total - Lost; }
            }

            public static CombatGroupData Create(CombatGroup group, List<ResolveMessage> messages, bool isAttacker)
            {
                var leaderStats = Helpers.FormatLeaderStats(group.Leader);

                var lost = 0;
                var committed = 0;
                // var situation = group.Situation;
                var situationDelta = 0f;

                var categoryMap = new Dictionary<ElementCategory, StrengthStats>();

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

                    foreach(var g in sideMessage.UnitsCommitted.GroupBy(unit => unit.Unit.Type.Category))
                    {
                        if (!categoryMap.TryGetValue(g.Key, out var value))
                            categoryMap[g.Key] = value = new StrengthStats();
                        foreach(var unit in g)
                        {
                            value.Lost += unit.Lost;
                            value.Committed += unit.Committed;
                        }
                    }
                    // TODO: Use category value to compute "total" value directly
                }

                foreach(var g in group.Units.GroupBy(u => u.Type.Category))
                {
                    if(!categoryMap.TryGetValue(g.Key, out var value))
                        categoryMap[g.Key] = value = new StrengthStats();
                    foreach(var unit in g)
                        value.Total += unit.Strength + value.Lost;
                }

                var total = group.Units.Sum(u => u.Strength) + lost;

                var totalTactic = 0; // TODO: Add tactic modifier
                var situation = group.Situation - situationDelta;
                var moveSpeedModifier = 0; // TODO: Add Move Speed Modifier

                float beginPotentialChance = -1;
                float beginBaselineChance = -1;

                if(messages.Count >= 1)
                {
                    var combatSide0 = isAttacker ? messages[0].Combat.Attacker : messages[0].Combat.Defender;
                    beginPotentialChance = combatSide0.BeginChance.Potential;
                    beginBaselineChance = combatSide0.BeginChance.Baseline;
                }

                var relatedMessages = messages.Where(m => (m.Combat.AttackerInitiative && isAttacker) || (!m.Combat.AttackerInitiative && !isAttacker)).ToList();
                var typeCounter = relatedMessages.GroupBy(m => m.Combat.Type).Select(g => (g.Key, g.Count())).ToArray();
                var resultSummaryCounter = relatedMessages.GroupBy(m => m.Result.ResultSummary).Select(g => (g.Key, g.Count())).ToArray();

                return new()
                {
                    LeaderPortrait= Helpers.GetSprite(group.Leader, group.Side),
                    LeaderDescription=$"{group.Leader.Name}({leaderStats})",
                    Total=total,
                    Lost=lost,
                    Committed= committed,
                    TotalTactic= totalTactic,
                    Situation= situation,
                    SituationDelta=situationDelta,
                    MoveSpeedModifier= moveSpeedModifier,
                    BeginPotentialChance= beginPotentialChance,
                    BeginBaselineChance= beginBaselineChance,
                    TypeCounter = typeCounter,
                    ResultSummaryCounter= resultSummaryCounter,
                    CategoryStatsMap= categoryMap
                };
            }
        }
    }

    void SyncCombatGroup(SymmetryUI ui, SnapshotData.CombatGroupData groupData)
    {
        ui.LeaderPortrait.style.backgroundImage = new StyleBackground(groupData.LeaderPortrait);
        ui.LeaderStats.text = groupData.LeaderDescription;
        var chanceS = groupData.BeginBaselineChance == -1 ? "-/-" : $"{groupData.BeginPotentialChance.ToString("N0")}/{groupData.BeginBaselineChance.ToString("N0")}";

        ui.TextSummary.text = @$"{groupData.Total} men (-{groupData.Lost})
Committed: {groupData.Committed} pts
Total Tactic: {groupData.TotalTactic} (0%)
Situation: {groupData.Situation.ToString("P2")} ({groupData.SituationDelta.ToString("P2")})
Move Speed: {groupData.MoveSpeedModifier.ToString("P2")}
Chance: {chanceS}";

        ui.SubCombatTypes.text = string.Join(",", groupData.TypeCounter.Select(tv=>$"{tv.Item1}:{tv.Item2}"));
        ui.SubCombatResults.text = string.Join(",", groupData.ResultSummaryCounter.Select(rv => $"{rv.Item1.ShortName}:{rv.Item2}"));
    }
}
