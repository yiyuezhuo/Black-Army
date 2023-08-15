using System.Collections.Generic;
using System;
using System.Linq;

namespace YYZ.BlackArmy.Model
{
    public enum PersonelState
    {
        Active,
        Killed,
        Captured,
        Exiled
    }

    public class Leader
    {
        public string Name;

        public string Trait;

        public int Strategic;
        public int Operational;
        public int Tactical;
        public int Guerrilla;
        public int Political;

        public string Wiki; // TODO: Lift it from the model?

        public bool IsPlaceholdLeader; // PlaceholdLeader don't have kill/capture check

        public PersonelState State;

        public Detachment Detachment;

        // public int Experience;
        // public int Battles;
        public override string ToString()
        {
            return $"Leader({Name})";
        }
    }

    public class ElementCategory
    {
        public string Name;
        // TODO: Use Tags
        public int Priority;
    }

    public class ElementType
    {
        public string Name; // required
        public ElementCategory Category;
        public float AllocationCoef = 1;
        public Dictionary<ElementType, float> AttachCoefMap = new(); // MachineGun is attached to infantry and cavalry
        public bool IsAttachment() => AttachCoefMap.Count > 0; // MachineGun
        // public bool Support; // Gun, ArmoredTrain
        public override string ToString()
        {
            return $"ElementType({Name}, {Category.Name}, AllocationCoef={AllocationCoef}, AttachCoefMap=[{AttachCoefMap.Count}], {IsAttachment()})";
        }
        public float FireSoft;
        public float FireHard;
        // public float Fire;
        // public float HardAttack;
        // public float Assault;
        
        public float AssaultAttack;
        public float AssaultDefense;
        public float Width;
        public float ArmorValue;
        
        public float Defense;
        // public float HitPoint=1;// Normal Infantry/Cavalry = 1, gun / tank = 0.1
        // public bool HardTarget; // armored car/train
        public int Morale; // A => 6, B => 5, C => 4, D => 3, E => 2, F => 1
        public int Manpower;
        public float Speed;
        public float TacticalSpeedModifier;
    }

    public class ElementTypeSystem
    {
        public List<ElementType> ElementTypes;
        public Dictionary<string, ElementType> Name2Type;
        public ElementTypeSystem(IEnumerable<ElementType> types)
        {
            ElementTypes = types.ToList();
            Name2Type = ElementTypes.ToDictionary(t => t.Name, t => t);
        }
        public IEnumerable<ElementType> IterNonAttachments() => ElementTypes.Where(e => !e.IsAttachment());
        public IEnumerable<ElementType> IterAttachments() => ElementTypes.Where(e => e.IsAttachment());
        public void Add(ElementType type)
        {
            ElementTypes.Add(type);
            Name2Type[type.Name] = type;
        }

        public ElementType GetType(int idx) => ElementTypes[idx];
        public ElementType GetType(string name) => Name2Type[name];

        public override string ToString()
        {
            return $"ElementTypeSystem([{Count}])";
        }

        public int Count{get => ElementTypes.Count;}
    }

    public class ElementValue
    {
        public int Strength;
        public ElementValue Copy() => new ElementValue(){Strength=Strength};
    }

    public class ElementContainer
    {
        public Dictionary<ElementType, ElementValue> Elements = new();

        // public Dictionary<ElementType, ElementValue> CopyElements() => Elements.ToDictionary(KV=>KV.Key, KV=>KV.Value.Copy());

        public ElementContainer Copy() => new ElementContainer(){Elements=Elements.ToDictionary(KV=>KV.Key, KV=>KV.Value.Copy())};
        public ElementContainer CopyNonAttachments() => new ElementContainer(){Elements=Elements.Where(e => !e.Key.IsAttachment()).ToDictionary(KV=>KV.Key, KV=>KV.Value.Copy())};
        // public ElementContainer CopyAttachments() => new ElementContainer(){Elements=Elements.Where(e => e.Key.IsAttachment()).ToDictionary(KV=>KV.Key, KV=>KV.Value.Copy())};

        public float MinSpeed() => Elements.Keys.Min(k => k.Speed);

        public override string ToString()
        {
            var s = string.Join(", ", Elements.Select(KV => $"{KV.Key.Name}:{KV.Value.Strength}"));
            return $"ElementContainer({s})";
            // return $"Formation(Infantry={Infantry}, Cavalry={Cavalry}, MachineGun={MachineGun}, Gun={Gun}, Tachanka={Tachanka}, ArmoredCar={ArmoredCar}, ArmoredTrain={ArmoredTrain})";
        }

        public int GetTotalManpower() => Elements.Sum(KV => KV.Key.Manpower * KV.Value.Strength);

        public void TransferTo(ElementContainer dst, ElementType type, int strength)
        {
            Minus(type, strength);
            dst.Add(type, strength);
        }

        public void Add(ElementType type, int strength)
        {
            if(Elements.TryGetValue(type, out var value))
            {
                value.Strength += strength;
            }
            else
            {
                Elements[type] = new ElementValue(){Strength=strength};
            }
        }

        public void Minus(ElementType type, int strength)
        {
            Elements[type].Strength -= strength;
            if(Elements[type].Strength <= 0)
                Elements.Remove(type);
        }

        public void Set(ElementType type, int strength)
        {
            if(strength <= 0)
                Elements.Remove(type);
            else
                Elements[type].Strength = strength;
        }

        public bool Contains(ElementType type)
        {
            if(Elements.TryGetValue(type, out var value))
            {
                return value.Strength > 0;
            }
            return false;
        }

        public int StrengthOf(ElementType type)
        {
            if(Elements.TryGetValue(type, out var value))
            {
                return value.Strength;
            }
            return 0;
        }

        public IEnumerable<ElementContainer> DivideNonAttachment(ElementTypeSystem system, int n, float minThreshold, float maxThreshold)
        {
            var currentContainer = CopyNonAttachments();
            var nonAttachments = system.IterNonAttachments().ToArray();
            var elementIdx = nonAttachments.Length - 1;

            while(n > 0)
            {
                if(n == 1)
                {
                    yield return currentContainer;
                }
                else
                {
                    var ptSum = currentContainer.Elements.Sum(KV => KV.Key.AllocationCoef * KV.Value.Strength);
                    var ptTarget = ptSum / n;

                    var allocatedContainer = new ElementContainer();
                    var ptAllocated = 0f;

                    for(; elementIdx >= 0; elementIdx--)
                    {
                        var element = nonAttachments[elementIdx];

                        if(!currentContainer.Contains(element))
                            continue;

                        var elementStrength = currentContainer.Elements[element].Strength;
                        var ptType = element.AllocationCoef * elementStrength;
                        var ptIfAllAllocated = ptType + ptAllocated;
                        if(ptIfAllAllocated < ptTarget * maxThreshold)
                        {
                            // allocatedContainer.Elements[element] = new ElementValue(){Strength=elementStrength};
                            currentContainer.TransferTo(allocatedContainer, element, elementStrength);
                            ptAllocated = ptIfAllAllocated;
                            if(ptIfAllAllocated > ptTarget * minThreshold)
                                break;
                        }
                        else
                        {
                            var allocatedStrength = (int)MathF.Ceiling(elementStrength * ptTarget / ptType);
                            currentContainer.TransferTo(allocatedContainer, element, allocatedStrength);
                            ptAllocated += allocatedStrength * element.AllocationCoef;
                            break;
                        }
                    }
                    yield return allocatedContainer;
                }
                n -= 1;
            }
        }

        public IEnumerable<ElementContainer> Divide(ElementTypeSystem system, int n, float minThreshold, float maxThreshold)
        {
            var attachments = system.IterAttachments().ToArray();

            var currentContainer = Copy();

            var remainPts = new Dictionary<ElementType, float>();

            foreach(var attachment in attachments)
            {
                foreach((var attachTarget, var coef) in attachment.AttachCoefMap)
                {
                    if(currentContainer.Contains(attachTarget))
                    {
                        if(!remainPts.ContainsKey(attachment))
                            remainPts[attachment] = 0;

                        remainPts[attachment] += currentContainer.Elements[attachTarget].Strength * coef;
                    }
                }
            }

            foreach(var container in DivideNonAttachment(system, n, minThreshold, maxThreshold))
            {
                foreach(var attachment in attachments)
                {
                    if(currentContainer.Contains(attachment))
                    {
                        var ptTarget = 0f;
                        foreach((var attachTarget, var coef) in attachment.AttachCoefMap)
                        {
                            if(container.Contains(attachTarget))
                            {
                                ptTarget += container.Elements[attachTarget].Strength * coef;
                            }
                        }

                        var attachmentStrength = currentContainer.Elements[attachment].Strength;
                        
                        if(remainPts[attachment] == 0) // If there're no infantry and cavalry, machine gun are not allocated
                            continue;
                        
                        var allocated = (int)MathF.Ceiling(attachmentStrength * ptTarget / remainPts[attachment]);
                        
                        if(allocated == 0)
                            continue;
                        
                        remainPts[attachment] -= ptTarget;
                        currentContainer.TransferTo(container, attachment, allocated);
                    }
                }
                yield return container;
            }
        }
    }

    public class Edge
    {
        public bool River;
        public bool Railroad;
        public bool CountryBoundary;
    }

    public class Side
    {
        public string Name;
        public float Morale;
        public float VP;
        public Leader PlaceholderLeader;
        public List<Detachment> Detachments = new();
        public bool RailroadMovementAvailable;

        public string PlaceholderLeaderName;
        public string PlaceholderLeaderTrait;
        // public string Tags;

        public override string ToString()
        {
            return $"Side({Name}, {Morale}, {VP}, RailroadMovementAvailable={RailroadMovementAvailable})";
        }
    }

    public class HexSideValue
    {
        public float Situation;
    }

    public class Hex
    {
        public int X;
        public int Y;
        public string Type;
        public Dictionary<Hex, Edge> EdgeMap = new();
        public List<Detachment> Detachments = new();
        public Dictionary<Side, HexSideValue> SideValueMap = new();

        public override string ToString()
        {
            return $"Hex({X}, {Y}, {Type}, Edges:[{EdgeMap.Count}], Detachments:[{Detachments.Count}])";
        }
    }

    public class MovingState
    {
        public Hex CurrentTarget;
        public float CurrentCompleted;
        public List<Hex> Waypoints; // Future waypoints, which don't include `CurrentTarget`
        public float CurrentRemainRange { get => GameParameters.HexDistance * (1 - CurrentCompleted); }

        public bool GotoNextWaypoint() // => end?
        {
            CurrentCompleted = 0;
            if (Waypoints.Count == 0)
                return true;
            CurrentTarget = Waypoints[0];
            Waypoints.RemoveAt(0);
            return false;
        }

        public Hex FinalTarget{ get => Waypoints.Count > 0 ? Waypoints[Waypoints.Count - 1] : CurrentTarget; }
    }

    public class RuleOfEngagement
    {
        public string Name;
        public float FireThreshold = 1f;
        public float AssaultThreshold = 2f;
        public float Aggressiveness;
    }

    public class Detachment
    {
        public string Name;
        public List<Leader> Leaders = new();
        public ElementContainer Elements = new();
        public Side Side;
        public Hex Hex;
        public RuleOfEngagement RuleOfEngagement;

        public Leader CurrentLeader { get => Leaders.Count >= 1 ? Leaders[0] : Side.PlaceholderLeader; }
        public MovingState MovingState; // null => the unit is not moving
        public float MinSpeed() => Elements.MinSpeed();
        public float RealSpeed() => RealSpeed(MinSpeed());
        public float RealSpeed(float minSpeed) => Side.RailroadMovementAvailable && Hex.EdgeMap[MovingState.CurrentTarget].Railroad ? GameParameters.RailroadSpeed : minSpeed;

        public override string ToString()
        {
            return $"Detachment({Name}, [{Leaders.Count}], [{Elements.Elements.Count}], {Side.Name}, {Hex})";
        }

        public void ResolveSubTurn()
        {
            var minSpeed = Elements.MinSpeed();
            if(MovingState != null)
            {
                var t = 1f / GameParameters.SubTurns;
                while(t > 0)
                {
                    var speed = RealSpeed(minSpeed);
                    var maxRange = speed * t; // TODO: Add terrain effects
                    var remainRange = MovingState.CurrentRemainRange; // TODO: add terrain effects
                    if(maxRange < remainRange)
                    {
                        MovingState.CurrentCompleted += (maxRange / remainRange);
                        break;
                    }
                    var usedP = remainRange / maxRange;
                    t = t * (1 - usedP);
                    MoveTo(MovingState.CurrentTarget);

                    var isCompleted = MovingState.GotoNextWaypoint();
                    if(isCompleted)
                    {
                        MovingState = null;
                        break;
                    }
                }
            }
        }

        public void MoveTo(Hex dst)
        {
            Hex.Detachments.Remove(this);
            Hex = dst;
            dst.Detachments.Add(this);
        }

        public int GetTotalManpower() => Elements.GetTotalManpower() + Leaders.Count; // TODO: Add Leader?

        public void TransferTo(Detachment dst, Leader leader)
        {
            Leaders.Remove(leader);
            dst.Leaders.Add(leader);
        }

        public bool IsEmpty() => Leaders.Count == 0 && Elements.Elements.Count == 0;
    }

    public static class GameParameters
    {
        public static int SubTurns = 10;
        public static float RailroadSpeed = 480; // 480km / day
        public static float HexDistance = 50; // 50 km / hex
    }

    public class GameState
    {
        public int Turn = 1;
        public List<Side> Sides;
        public List<Hex> Hexes;
        public Side CurrentSide;
        
        public DateTime BeginDateTime = new DateTime(1920, 11, 26);
        public DateTime CurrentDateTime{get => BeginDateTime + TimeSpan.FromDays(Turn - 1);}

        public List<ElementCategory> ElementCategories;
        public ElementTypeSystem ElementTypeSystem;

        
        public static List<RuleOfEngagement> RoEList = new() // [0] is the default one
        {
            new(){Name="Balanced", FireThreshold=1f, AssaultThreshold=2f, Aggressiveness=1},
            new(){Name="Aggressive", FireThreshold=0.5f, AssaultThreshold=1f, Aggressiveness=2},
            new(){Name="Passive", FireThreshold=1.5f, AssaultThreshold=3f, Aggressiveness=0}
        };
        

        public override string ToString()
        {
            return $"GameState({Turn}, {CurrentSide}, {CurrentDateTime})";
        }

        public IEnumerable<Detachment> Detachments
        {
            get
            {
                foreach(var side in Sides)
                {
                    foreach(var detachment in side.Detachments)
                    {
                        yield return detachment;
                    }
                }
            }
        }

        public void NextPhase()
        {
            // Toggle side and jump to next turn on occasion.
            var idx = Sides.IndexOf(CurrentSide);
            if(idx >= Sides.Count - 1)
            {
                ResolveTurn(); // Small step WEGO system, so it looks like Real-Time
                CurrentSide = Sides[0];
                Turn += 1;
            }
            else
            {
                CurrentSide = Sides[idx + 1];
            }
        }

        public void ResolveTurn()
        {
            // Turn Begin Processing

            // Successive Sub Turn Resolv
            for(var i=0; i<GameParameters.SubTurns; i++)
                ResolveSubTurn();
            
            // Turn End Processing
        }

        public void ResolveSubTurn()
        {
            foreach(var detachment in Detachments)
            {
                detachment.ResolveSubTurn();
            }
        }

        public Dictionary<Hex, Dictionary<Side, List<Detachment>>> GetHexSideDetachmentsMap()
        {
            var ret = new Dictionary<Hex, Dictionary<Side, List<Detachment>>>();

            foreach(var detachment in Detachments)
            {
                if (!ret.TryGetValue(detachment.Hex, out var sideDetachmentsMap))
                    sideDetachmentsMap = ret[detachment.Hex] = new();
                if(!sideDetachmentsMap.TryGetValue(detachment.Side, out var detachments))
                    detachments = sideDetachmentsMap[detachment.Side] = new();
                detachments.Add(detachment);
            }

            return ret;
        }

        public Dictionary<Hex, Dictionary<Side, int>> GetHexSideStrengthMap()
        {
            var ret = new Dictionary<Hex, Dictionary<Side, int>>();

            foreach (var detachment in Detachments)
            {
                if (!ret.TryGetValue(detachment.Hex, out var sideStrenghMap))
                    sideStrenghMap = ret[detachment.Hex] = new();
                if (!sideStrenghMap.TryGetValue(detachment.Side, out var currentStrength))
                    currentStrength = 0;
                sideStrenghMap[detachment.Side] = currentStrength + detachment.GetTotalManpower();
            }

            return ret;
        }
    }
}
