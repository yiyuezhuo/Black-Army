using System.Collections.Generic;
using YYZ.BlackArmy.Model;
using System;
using System.Linq;

using static YYZ.Helpers;
using System.Dynamic;

namespace YYZ.CombatGenerator
{

    public interface IUnit
    {
        public int Strength{get;}
        public float Width{get;}
        public float TacticSpeed{get;}
    }

    public interface ICombatGroup // attacker or defender, battle group
    {
        public IEnumerable<IUnit> Units{get;}

        // Leadership / order properties
        public float Command{get;} // Lerp of Operational and Guerrilla. 0~6, prevent ineffective deployment.
        public float Judgement{get;} // 0~6, Determine noise level used in the combat level decision. 
        
        // Quantification of the Rule Of Engagement and commander aggressiveness
        public float FireThreshold{get;} // reference: 1.0, 
        public float AssaultThreshold{get;} // reference: >2.0, 

        // Area/global properties
        public float Situation{get; set;} // -100%~100% (-1.0 ~ +1.0)
    }

    class Chance
    {
        float percent=1f;
        public float BeginPotential;
        public float BeginBaseline;

        public float Potential
        {
            get => BeginPotential * percent;
            set => percent = value / BeginPotential;
        }
        public float Baseline
        {
            get => BeginBaseline * percent;
            set => percent = value / BeginBaseline;
        }
        public float Clamp(float x) => MathF.Min(Potential, MathF.Min(Baseline, x));
    }

    public interface ICombatResult // Combat Generator receive result of every combat it generated
    {
        public float AttackerLostLevel{get;} // Victory = 0, Normal failure => 1, serious failure => > 1.0
        public float DefenderLostLevel{get;}
    }

    public class UnitDispatchCommand
    {
        public int Strength{get; set;}
    }

    public class CombatGroupDispatchCommand
    {
        public List<UnitDispatchCommand> Units;
    }

    public enum CombatType
    {
        Stand, // both side stand in its position or redeployment. 
        Fire, // direct example: JTS, AGEOD and PDX games. which represents attrition, skirmish, soft-up, probe attack etc. 
        Assault // the only decisive operation to seize situation superiority
    }

    public class GeneratedCombat
    {
        public CombatGroupDispatchCommand Attacker;
        public CombatGroupDispatchCommand Defender;
        public CombatType Type;
        public bool AttackerInitiative;

        public override string ToString()
        {
            var aStr = string.Join(",", Attacker.Units.Select(u=>u.Strength));
            var dStr = string.Join(",", Defender.Units.Select(u=>u.Strength));
            return $"GeneratedCombat({Type}, AI:{AttackerInitiative}, [{Attacker.Units.Count},{Defender.Units.Count}], {aStr} | {dStr})";
        }
    }

    public class CombatGenerator
    {
        public float BaselineTacticalSpeed = 25; // EX: 25km / day
        public float CombatChanceLowerLimit = 10;
        public float CombatChanceUpperLimit = 1000; // (1000 => Width 1 1000 men), BaselineTacticalSpeed free

        public ICombatResult CurrentCombatResult; // "Consumed" in the generator to produce state update
        public float BaseBattleProb = 0.75f;
        public float DensityEffectWidthUpperLimit = 10000f;
        public float CombatEffectCoef = 0.1f;
        public float CommandBase = 0.2f;
        public float JudgementBaseNoise = 0.1f;
        public float JudgementReferneceLevel = 6;
        public float JudgementNoiseCoef = 0.1f;
        public float CombatResultEffectCoef = 1f;
        public float passiveCorrelation = 0.2f;

        Chance MakeChance(ICombatGroup group)
        {
            return new Chance()
            {
                BeginPotential = group.Units.Sum(u => u.Strength * u.Width * u.TacticSpeed),
                BeginBaseline = group.Units.Sum(u => u.Strength * u.Width * BaselineTacticalSpeed)
            };
        }

        float RollCombatAsset(Chance chance)
        {
            var r = NextFloat() *(CombatChanceUpperLimit - CombatChanceLowerLimit) + CombatChanceLowerLimit;
            return chance.Clamp(r * BaselineTacticalSpeed);
            // return MathF.Min(chance.Baseline, MathF.Min(chance.Potential, r * BaselineTacticalSpeed));
            // return r * BaselineTacticalSpeed;
        }

        class CombatGroupWrapper
        {
            public ICombatGroup Group;
            public Chance Chance;
            public CombatGroupWrapper(ICombatGroup group, float baselineTacticalSpeed)
            {
                this.Group = group;
                this.Chance = MakeChance(group, baselineTacticalSpeed);
            }

            public float TotalWidth() => Group.Units.Sum(u => u.Strength * u.Width);
            Chance MakeChance(ICombatGroup group, float baselineTacticalSpeed)
            {
                return new Chance()
                {
                    BeginPotential = group.Units.Sum(u => u.Strength * u.Width * u.TacticSpeed),
                    BeginBaseline = group.Units.Sum(u => u.Strength * u.Width * baselineTacticalSpeed)
                };
            }

        }

        bool BaseProbCheck() => NextFloat() <= BaseBattleProb;
        bool DensityCheck(float minWidth) => NextFloat() <= minWidth / DensityEffectWidthUpperLimit;
        bool DeploymentCheck(float command) => NextFloat() <= CommandBase + command * CombatEffectCoef;
        bool SituationCheck(float situation) => NextFloat() <= situation / 2 + 0.5;
        float JudgementNoise(CombatGroupWrapper group)
        {
            var offset = JudgementReferneceLevel - group.Group.Judgement;
            // TODO: more symmeric noise to drive over-cautious or over-aggressive behaviour.
            var coef = MathF.Max(0, JudgementBaseNoise + offset * JudgementNoiseCoef);
            var noise = NextFloat() * 2 - 1;
            return 1 + coef * noise;
        }

        CombatType RollCombatTypeDecision(CombatGroupWrapper group, float ratio)
        {
            var ratioNoised = ratio * JudgementNoise(group);
            if(ratioNoised < group.Group.FireThreshold)
                return CombatType.Stand;
            else if(ratioNoised < group.Group.AssaultThreshold)
                return CombatType.Fire;
            return CombatType.Assault;
        }

        public IEnumerable<GeneratedCombat> Generate(ICombatGroup attackerGroup, ICombatGroup defenderGroup)
        {
            // The combat we modeled (Anarchist's campaign 1920~1921) doesn't show much defenders's superiority so we handle them symmetrically at this point.
            var attacker = new CombatGroupWrapper(attackerGroup, BaselineTacticalSpeed);
            var defender = new CombatGroupWrapper(defenderGroup, BaselineTacticalSpeed);

            while(attacker.Chance.Potential > 0 || defender.Chance.Potential > 0)
            {
                var ap = attacker.Chance.Potential;
                var dp = defender.Chance.Potential;
                var attackerInitiative = NextFloat() < ap / (ap + dp);

                (var active, var passive) = attackerInitiative ? (attacker, defender) : (defender, attacker);
                var activeAsset = RollCombatAsset(active.Chance);
                active.Chance.Potential -= activeAsset;
                if(!BaseProbCheck())
                    continue;
                
                var minWidth = MathF.Min(active.TotalWidth(), passive.TotalWidth());
                if(!DensityCheck(minWidth)) // Guerrila or low intensity maneuver warfare
                    continue;

                if(!DeploymentCheck(active.Group.Command)) // Commander's ability to deploy troop on proper location
                    continue;

                if(!SituationCheck(active.Group.Situation))
                    continue;

                var passiveAsset = RollCombatAsset(passive.Chance);

                passiveAsset = passive.Chance.Clamp((1 - passiveCorrelation) * passiveAsset + passiveCorrelation * activeAsset);

                if(passiveAsset == 0)
                    yield break;
                var ratio = activeAsset / passiveAsset; // Traditional CRT ratio

                var combatType = RollCombatTypeDecision(active, ratio);
                switch(combatType)
                {
                    case CombatType.Stand:
                        if(DeploymentCheck(active.Group.Command))
                            passive.Chance.Baseline -= MathF.Min(passiveAsset, MathF.Max(activeAsset, passiveAsset / 2)); // partial divergent
                        continue; // TODO: Explicitly model stand combat.
                    case CombatType.Fire:
                    case CombatType.Assault:
                        passive.Chance.Baseline -= passiveAsset;

                        var ad = MakeGroupDispatchCommand(activeAsset, active);
                        var pd = MakeGroupDispatchCommand(passiveAsset, passive);
                        (var attackDispatch, var defenderDispatch) = attackerInitiative ? (ad, pd) : (pd, ad);
                        yield return new GeneratedCombat()
                        {
                            Attacker=attackDispatch,
                            Defender=defenderDispatch,
                            Type=combatType,
                            AttackerInitiative=attackerInitiative
                        };
                        break;
                }

                if(CurrentCombatResult == null)
                    throw new ArgumentNullException("CurrentCombatResult should be assigned before re-enter");

                var r = CurrentCombatResult;
                CurrentCombatResult = null;

                (var activeLostLevel, var passiveLostLevel) = attackerInitiative ? (r.AttackerLostLevel, r.DefenderLostLevel) : ( r.DefenderLostLevel, r.AttackerLostLevel);

                active.Chance.Baseline -=  activeAsset * activeLostLevel * CombatResultEffectCoef;
                passive.Chance.Baseline -= passiveAsset * passiveLostLevel * CombatResultEffectCoef;
            }
        }

        CombatGroupDispatchCommand MakeGroupDispatchCommand(float asset, CombatGroupWrapper group)
        {
            // TODO: Implement Heterogeneity Dispatch 
            var percent = asset / group.Group.Units.Sum(u => u.Strength * u.Width * BaselineTacticalSpeed);
            percent = MathF.Min(1, percent);
            var unitDispatchList = group.Group.Units.Select(u => new UnitDispatchCommand()
            {
                Strength=RandomRound(percent*u.Strength)
            }).ToList();
            var groupDispatchCommand = new CombatGroupDispatchCommand(){Units=unitDispatchList};
            return groupDispatchCommand;
        }
    }
}