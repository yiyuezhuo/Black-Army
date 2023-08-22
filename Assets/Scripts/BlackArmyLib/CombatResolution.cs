using System;
using System.Collections.Generic;
using System.Linq;
using static YYZ.Helpers;

namespace YYZ.CombatResolution
{
    /*
    public enum CombatResultSummary
    {
        Repelled,
        Stalemate,
        SoftenUp,
        Fallback,
        Routed,
        Overrun
    }
    */

    public interface ICombatResultSummary
    {
        public string Name{get;}
    }


    public interface ICombatResultSummaryResolver<T> where T : ICombatResultSummary
    {
        public T Resolve(float attackerLossPercent, float denfenderLossPercent,
            float attackerMorale, float defenderMorale); //1 ~ 6 (F~A)
    }




    public interface IUnit
    {
        // type
        float FireSoft{get;}
        float FireHard{get;}
        float AssaultAttack{get;}
        float AssaultDefense{get;}
        float Defense{get;} // Like "HP", Line-Filler -> 1, 
        float ArmorValue{get;} // foot infantry -> 0, Tank -> 1, Mechanized -> 0~1 (though it's not use in Black Army)
        float Width{get;} // Like "weight" or stack limit, Line-Filler -> 1, Close support -> 0.5, Long range support -> 0.1

        // state
        int Strength{get;set;}
    }

    public interface ILossTable
    {
        float Draw();
    }

    public class LossTable: ILossTable // JTS style combat resolution for lower level resoluion, so we ensure strategic result is derived from a solid operational "simulator"
    {
        protected static Random rng = new Random();
        public float Low;
        public float High;
        protected float Draw1() => Low + (High - Low) * (float)rng.NextDouble();
        public virtual float Draw() => Draw1();
    }

    public class HitTableAverage: LossTable
    {
        public override float Draw() => (Draw1() + Draw1()) / 2;
    }

    public class UnitUpdate
    {
        public int ToStrength;
        public int StrengthDelta;

        public void ApplyTo(IUnit unit)
        {
            unit.Strength = ToStrength;
        }

        public override string ToString()
        {
            return $"UnitUpdate(delta={StrengthDelta})";
        }
    }


    public class CombatResult<T> where T : ICombatResultSummary
    {
        public List<UnitUpdate> AttackerUpdates;
        public List<UnitUpdate> DefenderUpdates;
        public T ResultSummary;

        static void ApplyTo(List<UnitUpdate> updates, IEnumerable<IUnit> units)
        {
            if(updates == null)
                return;
            foreach((var update, var unit) in updates.Zip(units, (x,y)=>(x,y)))
                update.ApplyTo(unit);
        }

        public void ApplyTo(IEnumerable<IUnit> attackers, IEnumerable<IUnit> defenders)
        {
            ApplyTo(AttackerUpdates, attackers);
            ApplyTo(DefenderUpdates, defenders);
        }

        public override string ToString()
        {
            return $"CombatResult({ResultSummary.Name}, [{AttackerUpdates.Count},{DefenderUpdates.Count}], {string.Join(",", AttackerUpdates)} | {string.Join(",", DefenderUpdates)})";
        }
    }

    public enum CombatMode
    {
        Fire,
        Assault
    }

    public class ConmbatSide
    {
        public IEnumerable<IUnit> Units;
        public float Morale;

        public override string ToString()
        {
            var l = Units.Select(unit => unit.Strength).ToList();
            var s = string.Join(",", l);
            return $"ConmbatSide(Morale={Morale}, [{l.Count}], {s})";
        }

        public (float, List<float>) GetWeights()
        {
            var widths = Units.Select(unit => unit.Strength * unit.Width).ToList();
            var totalWidth = widths.Sum();

            return (totalWidth, widths.Select(w => w / totalWidth).ToList());
        }

        public float GetAssaultAttack() => Units.Sum(unit => unit.AssaultAttack * unit.Strength);
        public float GetAssaultDefense() => Units.Sum(unit => unit.AssaultDefense * unit.Strength);
        public float GetFireHard() => Units.Sum(unit => unit.FireHard * unit.Strength);
        public float GetFireSoft() => Units.Sum(unit => unit.FireSoft * unit.Strength);
        public float GetArmorValue(float totalWidth) => Units.Sum(unit => unit.Strength * unit.Width * unit.ArmorValue) / totalWidth;

        public IEnumerable<UnitUpdate> CombatValueResolve(List<float> weights, float combatValue, ILossTable table)
        {
            // Weights are determined by width (line-filler is prefered and support is "protected") or strength (all unit suffer the same loss percent) or firepower (counter-battery)
            // var postCombatValue = combatValue * table.Draw();
            return Units.Zip(weights, (unit, weight) => GetUnitUpdate(unit, weight * combatValue, table));
        }

        UnitUpdate GetUnitUpdate(IUnit unit, float combatValue, ILossTable table)
        {
            var p = table.Draw();
            var postCombatValue = combatValue * p;

            /*
            if(unit.Defense == 0f) // TODO: remove the check once true cause is found
                throw new ArgumentOutOfRangeException($"unit.Defense={unit.Defense}");
            */

            var lossF = Math.Min(unit.Strength, postCombatValue / unit.Defense);
            
            // Console.WriteLine($"unit.Strength={unit.Strength}, postCombatValue={postCombatValue}, unit.Defense={unit.Defense}, lossF={lossF}");

            var loss = RandomRound(lossF);
            return new()
            {
                ToStrength = unit.Strength - loss,
                StrengthDelta = -loss
            };
        }

        public float GetToTotalWidth(IEnumerable<UnitUpdate> unitUpdates)
        {
            return Units.Zip(unitUpdates, (unit, update) => unit.Width * update.ToStrength).Sum();
        }

        public void ExecuteResult(IEnumerable<UnitUpdate> updates)
        {
            foreach((var unit, var update) in Units.Zip(updates, (x,y)=>(x,y)))
            {
                update.ApplyTo(unit);
            }
        }

        public float FireCombatValue(float targetAmorValue)
        {
            var atkSoftFire = GetFireSoft();
            var atkHardFire = GetFireHard();
            return atkHardFire * targetAmorValue + atkSoftFire * (1 - targetAmorValue);
        }
    }

    public class CombatSetting // where T : ICombatResultSummary
    {
        public ConmbatSide Attacker;
        public ConmbatSide Defender;
        public CombatMode Mode;

        public void ExecuteResult<T>(CombatResult<T> result) where T : ICombatResultSummary
        {
            Attacker.ExecuteResult(result.AttackerUpdates);
            Defender.ExecuteResult(result.DefenderUpdates);
        }

        public override string ToString()
        {
            return $"CombatSetting({Mode}, {Attacker}, {Defender})";
        }
    }

    public class CombatResolver<T> where T : ICombatResultSummary
    {
        // TODO: Extra Parameters
        public ILossTable AssaultLossTable;
        public ILossTable FireLossTable;
        public ICombatResultSummaryResolver<T> ResultResolver;

        public CombatResult<T> Resolve(CombatSetting setting)
        {
            (var attacker, var defender) = (setting.Attacker, setting.Defender);

            (var atkTotalWidth, var atkWeights) = attacker.GetWeights();
            (var defTotalWidth, var defWeights) = defender.GetWeights();

            float atkCombatValue, defCombatValue;
            switch(setting.Mode)
            {
                case CombatMode.Fire:
                    atkCombatValue = attacker.FireCombatValue(defender.GetArmorValue(defTotalWidth));
                    defCombatValue = defender.FireCombatValue(attacker.GetArmorValue(atkTotalWidth));
                    break;
                case CombatMode.Assault:
                    atkCombatValue = attacker.GetAssaultAttack();
                    defCombatValue = defender.GetAssaultDefense();
                    break;
                default:
                    throw new ArgumentException($"Unknown Mode: {setting.Mode}");
            }

            /*
            Console.WriteLine($"defCombatValue={defCombatValue}, atkCombatValue={atkCombatValue}, atkWeights={string.Join(",", atkWeights)}, defWeights={string.Join(",", defWeights)}");
            if(float.IsNaN(atkWeights[0]))
                Console.WriteLine("NaN");
            */
            
            var atkUpdates = attacker.CombatValueResolve(atkWeights, defCombatValue, AssaultLossTable).ToList();
            var defUpdates = defender.CombatValueResolve(defWeights, atkCombatValue, AssaultLossTable).ToList();

            var toAtkTotalWidth = attacker.GetToTotalWidth(atkUpdates);
            var toDefTotalWidth = defender.GetToTotalWidth(defUpdates);
            var atkLossPercent = 1 - toAtkTotalWidth / atkTotalWidth;
            var defLossPercent = 1 - toDefTotalWidth / defTotalWidth;
            var res = ResultResolver.Resolve(atkLossPercent, defLossPercent, attacker.Morale, defender.Morale);
            // UnityEngine.Debug.Log($"res={res}");
            return new()
            {
                AttackerUpdates=atkUpdates, DefenderUpdates=defUpdates, ResultSummary=res
            };
        }
    }

    public static class Examples
    {
        public interface ICombatResultLowerLimitSummary : ICombatResultSummary
        {
            public float LowerLimit{get;}
        }

        public class CombatResultSummary: ICombatResultLowerLimitSummary
        {
            public string Name{get; set;}
            public float LowerLimit{get; set;}
            public override string ToString()
            {
                return $"CombatResultSummary({Name}, {LowerLimit})";
            }
        }

        public class CombatResultSummaryResolver<T>: ICombatResultSummaryResolver<T> where T : ICombatResultLowerLimitSummary
        {
            // static Random rng = new();

            public List<T> AttackerLostResults; // Stalemate (Is not explicitly represented), Repelled
            public List<T> DefenderLostResults; // Stalemate (sentinel), SoftenUp (2nd required), Fallback, Routed, Overrun
            public float MoraleCoef = 0.2f;
            public float NoiseCoef = 0.2f;

            public T Resolve(float attackerLossPercent, float denfenderLossPercent,
                float attackerMorale, float defenderMorale //1 ~ 6 (F~A)
                )
            {
                var ap = GetModifiedPercent(attackerLossPercent, attackerMorale);
                var aIdx = AttackerLostResults.FindIndex((res) => ap < res.LowerLimit);
                // UnityEngine.Debug.Log($"attackerLossPercent={attackerLossPercent}, ap={ap}, aIdx={aIdx}, attackerMorale={attackerMorale}");
                if(aIdx == -1)
                    return AttackerLostResults[^1];
                else if(aIdx >= 1)
                    return AttackerLostResults[aIdx - 1];
                
                var dp = GetModifiedPercent(denfenderLossPercent, defenderMorale);
                var dIdx = DefenderLostResults.FindIndex((res) => dp < res.LowerLimit);
                // UnityEngine.Debug.Log($"denfenderLossPercent={denfenderLossPercent}, dp={dp}, dIdx={dIdx}, defenderMorale={defenderMorale}");
                if (dIdx == -1)
                    return DefenderLostResults[^1];
                return DefenderLostResults[dIdx - 1];
            }

            float GetModifiedPercent(float percent, float morale)
            {
                return percent * (1 + (6 - morale) * MoraleCoef) * (1 + NextFloat() * NoiseCoef);
            }
        }

        public static CombatResultSummaryResolver<CombatResultSummary> GetCombatResultSummaryResolver()
        {
            return new()
            {
                AttackerLostResults = new(){
                    new(){Name="Repelled", LowerLimit=0.2f}
                },
                DefenderLostResults = new(){
                    new(){Name="Stalemate", LowerLimit= -1f},
                    new(){Name="Soften", LowerLimit= 0.125f},
                    new(){Name="Fallback", LowerLimit= 0.25f},
                    new(){Name="Breakthrough", LowerLimit= 0.5f},
                    new(){Name="Overrun", LowerLimit= 1f},
                }
            };
        }

        public static CombatResolver<CombatResultSummary> GetCombatResolver()
        {
            return new()
            {
                AssaultLossTable=new LossTable(){Low=0.02f, High=0.1f},
                FireLossTable=new LossTable(){Low=0.01f, High=0.05f},
                ResultResolver=GetCombatResultSummaryResolver()
            };
        }

        public class UnitType
        {
            public float FireSoft;
            public float FireHard;
            public float AssaultAttack;
            public float AssaultDefense;
            public float Defense; // Like "HP", Line-Filler -> 1, 
            public float ArmorValue; // foot infantry -> 0, Tank -> 1, Mechanized -> 0~1 (though it's not use in Black Army)
            public float Width; // Like "weight" or stack limit, Line-Filler -> 1, Close support -> 0.5, Long range support -> 0.1
        }

        public class UnitState
        {
            public int Strength;
        }

        public class Unit: IUnit
        {
            public UnitState State;
            public UnitType Type;

            public float FireSoft{get => Type.FireSoft;}
            public float FireHard{get=> Type.FireHard;}
            public float AssaultAttack{get=> Type.AssaultAttack;}
            public float AssaultDefense{get=> Type.AssaultDefense;}
            public float Defense{get=> Type.Defense;} // Like "HP", Line-Filler -> 1, 
            public float ArmorValue{get=> Type.ArmorValue;} // foot infantry -> 0, Tank -> 1, Mechanized -> 0~1 (though it's not use in Black Army)
            public float Width{get=> Type.Width;} // Like "weight" or stack limit, Line-Filler -> 1, Close support -> 0.5, Long range support -> 0.1

            public int Strength{get=> State.Strength; set => State.Strength=value;}
        }
    }
}
