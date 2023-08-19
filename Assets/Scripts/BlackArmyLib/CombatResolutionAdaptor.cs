
using YYZ.BlackArmy.Model;
using System;
using System.Linq;
using System.Collections.Generic;

using static YYZ.Helpers;
using YYZ.CombatGenerator;

namespace YYZ.BlackArmy.CombatResolution
{
    public class Unit : CombatGenerator.IUnit
    {
        public ElementContainer Container;
        public ElementType Type;

        public int Strength{get => Container.StrengthOf(Type); set => Container.Set(Type, value);}
        public float Width{get => 1;} // TODO: Import Width data table
        public float TacticSpeed{get => Type.Speed * Type.TacticalSpeedModifier;}
    }

    public class UnitCommitted : YYZ.CombatResolution.IUnit
    {
        public Unit Unit;
        public int Committed;
        public int Lost = 0;

        public override string ToString()
        {
            return $"UnitCommited({Unit.Strength}/{Committed}/{Lost})";
        }

        public UnitCommitted(Unit unit, int commited)
        {
            Unit = unit;
            Committed = commited;
        }

        public int Strength
        {
            get => Committed - Lost;
            set
            {
                Lost = Committed - value;
                Unit.Strength = Unit.Strength - Lost;
            }
        }
        public float FireSoft{get => Unit.Type.FireSoft;}
        public float FireHard{get => Unit.Type.FireHard;}
        // public float FireSoft{get => Unit.Type.Fire;}
        // public float FireHard{get => Unit.Type.Fire;} // TODO: Add FireHard value in data tables.
        public float AssaultAttack{get => Unit.Type.AssaultAttack;}
        public float AssaultDefense{get => Unit.Type.AssaultDefense;}
        // public float AssaultAttack{get => Unit.Type.Assault;}
        // public float AssaultDefense{get => Unit.Type.Assault;} // TODO: Determine using asymmetric resolution table or use asymmetric atd/def assault value.
        public float Defense{get => Unit.Type.Defense;}
        public float ArmorValue{get => Unit.Type.ArmorValue;}
        // public float ArmorValue{get => 0;} // TODO: Add ArmorValue or "IsHardTarget" properties into data tables.
        // public float Width{get => 1;} // TODO: Import Width data table
        public float Width{get => Unit.Type.Width;}
    }

    public class CombatGroup: CombatGenerator.ICombatGroup
    {
        public List<Unit> Units;
        IEnumerable<CombatGenerator.IUnit> CombatGenerator.ICombatGroup.Units{get => this.Units;}

        public float Command{get;set;} // Lerp of Operational and Guerrilla. 0~6, prevent ineffective deployment.
        public float Judgement{get;set;} // 0~6, Determine noise level used in the combat level decision. 
        
        // Quantification of the Rule Of Engagement and commander aggressiveness
        public float FireThreshold{get;set;} // reference: 1.0, 
        public float AssaultThreshold{get;set;} // reference: >2.0, 

        // Area/global properties
        public float Situation{get => Hex.SideValueMap[Side].Situation; set => Hex.SideValueMap[Side].Situation = value;} // -100%~100% (-1.0 ~ +1.0)
        // Situation=hex.SideValueMap[side].Situation,

        // Extra properties
        public RuleOfEngagement RuleOfEngagement; 
        public Side Side;
        public Hex Hex;
        public Leader Leader;
        
    }

    public class CombatResultSummary: YYZ.CombatResolution.Examples.ICombatResultLowerLimitSummary
    {
        public string Name{get; set;}
        public float LowerLimit{get; set;}
        public float AttackerLostLevel{get; set;}
        public float DefenderLostLevel{get; set;}
        public float AttackerSituationEffect; // 0 -> 0 effect, 1 -> normal retreat effect.
        public float DefenderSituationEffect;
        public string ShortName;

        public override string ToString()
        {
            return $"CombatResultSummary({Name}, {LowerLimit}, {AttackerLostLevel}, {DefenderLostLevel}, AttackerSituationEffect={AttackerSituationEffect}, DefenderSituationEffect={DefenderSituationEffect}, ShortName={ShortName})";
        }
    }

    public class SubCombatResultSummaryWrapper : CombatGenerator.ICombatResult
    {
        public CombatResultSummary Summary;
        public bool AttackerInitiative;
        public float AttackerLostLevel{get => AttackerInitiative ? Summary.AttackerLostLevel : Summary.DefenderLostLevel;}
        public float DefenderLostLevel{get => AttackerInitiative ? Summary.DefenderLostLevel : Summary.AttackerLostLevel;}
    }


    public class Resolver
    {
        public float GuerillaThreshold = 10_000;
        public float SituationBaseCoef = 5f; // 5 => ( 20% Withdraw (x1 effect) => -100% Situation.)

        public CombatGroup AttackerGroup;
        public CombatGroup DefenderGroup;
        public Hex Hex;

        public Resolver(Hex hex)
        {
            (AttackerGroup, DefenderGroup) = CreateGroup(hex);
            Hex = hex;
        }

        (YYZ.CombatResolution.ConmbatSide, List<UnitCommitted>) ApplyCommand(CombatGenerator.CombatGroupDispatchCommand groupCommand, IEnumerable<Unit> units)
        {
            var unitsCommited = groupCommand.Units.Zip(units, (command, unit) => new UnitCommitted(unit, command.Strength)).ToList();
            var manpower = unitsCommited.Sum(u => u.Unit.Type.Manpower * u.Strength);
            var morale = unitsCommited.Sum(u => u.Unit.Type.Manpower * u.Strength * u.Unit.Type.Morale) / manpower;
            var combatSide = new YYZ.CombatResolution.ConmbatSide(){Units=unitsCommited, Morale=morale};
            return (combatSide, unitsCommited);
        }

        public class CombatTypeConfig
        {
            public string Name;
            public YYZ.CombatResolution.CombatMode Mode; // TODO: refactor
        }

        public static Dictionary<CombatGenerator.CombatType, CombatTypeConfig> CombatTypeConfigMap = new()
        {
            {CombatGenerator.CombatType.Fire, new CombatTypeConfig()
                {Name="Fire", Mode=YYZ.CombatResolution.CombatMode.Fire}},
            {CombatGenerator.CombatType.Assault, new CombatTypeConfig()
                {Name="Assault", Mode=YYZ.CombatResolution.CombatMode.Assault}},
        };

        public static YYZ.CombatResolution.Examples.CombatResultSummaryResolver<CombatResultSummary> SummaryResolver = new()
        {
            AttackerLostResults = new(){
                new(){Name="Repelled",      ShortName="RE",  LowerLimit=0.2f,    AttackerLostLevel=1f,       DefenderLostLevel=0f,       AttackerSituationEffect=0f, DefenderSituationEffect=0}
            },
            DefenderLostResults = new(){
                new(){Name="Stalemate",     ShortName="ST", LowerLimit= -1f,    AttackerLostLevel=0.5f,     DefenderLostLevel=0.25f,    AttackerSituationEffect=0f, DefenderSituationEffect=0},
                new(){Name="Soften",        ShortName="SO", LowerLimit= 0.125f, AttackerLostLevel=0.5f,     DefenderLostLevel=0.5f,     AttackerSituationEffect=0f, DefenderSituationEffect=0.2f},
                new(){Name="Fallback",      ShortName="FA", LowerLimit= 0.25f,  AttackerLostLevel=0.25f,    DefenderLostLevel=0.75f,    AttackerSituationEffect=0f, DefenderSituationEffect=1f},
                new(){Name="Breakthrough",  ShortName="BR", LowerLimit= 0.5f,   AttackerLostLevel=0f,       DefenderLostLevel=1f,       AttackerSituationEffect=0f, DefenderSituationEffect=3f},
                new(){Name="Overrun",       ShortName="OV", LowerLimit= 1f,     AttackerLostLevel=0f,       DefenderLostLevel=2f,       AttackerSituationEffect=0f, DefenderSituationEffect=5f},
            }
        };

        public static YYZ.CombatResolution.CombatResolver<CombatResultSummary> SubCombatResolver = new()
        {
            AssaultLossTable=new YYZ.CombatResolution.LossTable(){Low=0.02f, High=0.1f},
            FireLossTable=new YYZ.CombatResolution.LossTable(){Low=0.01f, High=0.05f},
            ResultResolver=SummaryResolver
        };

        float UpdateSituation(CombatGroup group, List<UnitCommitted> unitCommited, CombatResultSummary summary, bool isActive)
        {
            var p = unitCommited.Sum(u => u.Lost * u.Unit.Width) / unitCommited.Sum(u => u.Unit.Strength * u.Unit.Width);
            var coef = isActive ? summary.AttackerSituationEffect : summary.DefenderSituationEffect;
            var delta = -p * SituationBaseCoef * coef;
            group.Situation += delta;
            return delta;
        }

        public class ResolveMessage
        {
            public class SideMessage
            {
                public float SituationDelta;
                public List<UnitCommitted> UnitsCommitted;
                public override string ToString()
                {
                    var s = string.Join(",", UnitsCommitted);
                    return $"SideMessage({SituationDelta}, {s})";
                }
            }

            public CombatGenerator.GeneratedCombat Combat;
            public YYZ.CombatResolution.CombatResult<CombatResultSummary> Result;
            public SideMessage Attacker;
            public SideMessage Defender;

            public override string ToString()
            {
                return $"ResolveMessage({Combat}, {Result}, {Attacker}, {Defender})";
            }
        }

        public IEnumerable<ResolveMessage> Resolve()
        {
            var combatGenerator = new CombatGenerator.CombatGenerator();
            foreach(var combat in combatGenerator.Generate(AttackerGroup, DefenderGroup))
            {
                (var attacker, var atkCommitedUnits) = ApplyCommand(combat.Attacker, AttackerGroup.Units);
                (var defender, var defCommitedUnits) = ApplyCommand(combat.Defender, DefenderGroup.Units);
                var config = CombatTypeConfigMap[combat.Type];

                (var active, var passive) = combat.AttackerInitiative ? (attacker, defender) : (defender, attacker);
                var resolveSetting = new YYZ.CombatResolution.CombatSetting(){Attacker= active, Defender= passive, Mode=config.Mode};
                
                var res = SubCombatResolver.Resolve(resolveSetting);
                
                combatGenerator.CurrentCombatResult = new SubCombatResultSummaryWrapper(){Summary=res.ResultSummary, AttackerInitiative=combat.AttackerInitiative};
                res.ApplyTo(active.Units, passive.Units);
                // res.ResultSummary.
                // var ap = atkCommitedUnits.Sum(u => u.Lost * u.Unit.Width) / atkCommitedUnits.Sum(u => u.Commited * u.Unit.Width);
                var asDelta = UpdateSituation(AttackerGroup, atkCommitedUnits, res.ResultSummary, combat.AttackerInitiative);
                var dsDelta = UpdateSituation(DefenderGroup, defCommitedUnits, res.ResultSummary, !combat.AttackerInitiative);

                yield return new()
                {
                    Combat=combat, Result=res, 
                    Attacker=new(){SituationDelta=asDelta, UnitsCommitted=atkCommitedUnits},
                    Defender=new(){SituationDelta=dsDelta, UnitsCommitted=defCommitedUnits},
                };
            }
        }

        public (CombatGroup, CombatGroup) CreateGroup(Hex hex)
        {
            var gb = hex.Detachments.GroupBy(d => d.Side);
            var minStrength = gb.Select(g => g.Sum(d=>d.GetTotalManpower())).Min();
            
            var guerillaLevel = 1 - MathF.Min(1, minStrength / GuerillaThreshold);

            var combatGroups = new List<CombatGroup>();
            foreach(var g in gb)
            {
                var side = g.Key;
                var units = g.SelectMany(d => d.Elements.Elements.Select(
                    KV => new Unit(){Container=d.Elements, Type=KV.Key})
                    ).ToList();
                // g.Min(d => d.GetTotalManpower())

                var maxDetachment = MaxBy(g, d=>d.GetTotalManpower());
                var leader = maxDetachment.CurrentLeader;
                var command = leader.Guerrilla * guerillaLevel + leader.Operational * (1 - guerillaLevel);
                var roe = maxDetachment.RuleOfEngagement;

                var combatGroup = new CombatGroup()
                {
                    Units=units,
                    Command=command,
                    Judgement=command,
                    FireThreshold=roe.FireThreshold,
                    AssaultThreshold=roe.AssaultThreshold,
                    // Situation=hex.SideValueMap[side].Situation,
                    // extra
                    RuleOfEngagement=maxDetachment.RuleOfEngagement,
                    Side=side,
                    Hex=hex,
                    Leader=leader
                };
                combatGroups.Add(combatGroup);
            }

            if(combatGroups.Count != 2)
                throw new ArgumentException($"combatGroup is expected to be 2, but {combatGroups.Count} is given");

            var a0 = combatGroups[0].RuleOfEngagement.Aggressiveness;
            var a1 = combatGroups[1].RuleOfEngagement.Aggressiveness;
            CombatGroup attackerGroup, defenderGroup;
            if(a0 == a1)
                (attackerGroup, defenderGroup) = NextFloat() > 0.5f ? (combatGroups[0], combatGroups[1]) : (combatGroups[1], combatGroups[0]);
            else if(a0 > a1)
                (attackerGroup, defenderGroup) = (combatGroups[0], combatGroups[1]);
            else
                (attackerGroup, defenderGroup) = (combatGroups[1], combatGroups[0]);
            
            // var generator = new CombatGenerator.CombatGenerator();
            // return generator.Generate(attackerGroup, defenderGroup);
            return (attackerGroup, defenderGroup);
        }

        static T MaxBy<T>(IEnumerable<T> collection, Func<T, int> f)
        {
            var max = int.MinValue;
            var maxEl = default(T);
            foreach(var el in collection)
            {
                var x = f(el);
                if(x > max)
                {
                    max = x;
                    maxEl = el;
                }
            }
            return maxEl;
        }
    }


}