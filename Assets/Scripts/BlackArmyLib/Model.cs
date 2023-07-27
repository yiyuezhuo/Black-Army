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
        public int Strategic;
        public int Operational;
        public int Tactical;
        public int Guerilla;
        public int Political;

        public PersonelState State;

        // public int Experience;
        // public int Battles;
    }

    public class ElementType
    {
        public string Name; // required
        public float AllocationCoef = 1;
        public Dictionary<ElementType, float> AttachCoefMap = new(); // MachineGun is attached to infantry and cavalry
        public bool IsAttachment() => AttachCoefMap.Count > 0; // MachineGun
        public bool Support; // Gun, ArmoredTrain
        public override string ToString()
        {
            return $"ElementType({Name})";
        }

        public float SoftAttack;
        public float HardAttack;
        public float Assault;
        public float Defense;
        public float HitPoint=1;// Normal Infantry/Cavalry = 1, gun / tank = 0.1
        public bool HardTarget; // armored car/train
    }

    public class ElementTypeSystem
    {
        public List<ElementType> ElementTypes;
        public Dictionary<string, ElementType> Name2Type;
        public ElementTypeSystem(IEnumerable<ElementType> types)
        {
            ElementTypes = types.ToList();
            Name2Type = types.ToDictionary(t => t.Name, t => t);
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

        public override string ToString()
        {
            var s = string.Join(", ", Elements.Select(KV => $"{KV.Key.Name}:{KV.Value.Strength}"));
            return $"ElementContainer({s})";
            // return $"Formation(Infantry={Infantry}, Cavalry={Cavalry}, MachineGun={MachineGun}, Gun={Gun}, Tachanka={Tachanka}, ArmoredCar={ArmoredCar}, ArmoredTrain={ArmoredTrain})";
        }

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

        public bool Contains(ElementType type)
        {
            if(Elements.TryGetValue(type, out var value))
            {
                return value.Strength > 0;
            }
            return false;
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

    public class Unit
    {
        public List<Leader> Leaders;

        public ElementContainer Elements;

        /*
        static float InfantryStrengthCoef = 1f;
        static float CavalryStrengthCoef = 1.25f;
        static float TachankaStrengthCoef = 125f;
        static float GunStrengthCoef = 125f;
        static float ArmoredCarStrengthCoef = 125f;
        static float ArmoredTrainStrengthCoef = 1000f;

        static float minAllocationPercent = 0.5f;
        static float maxAllocationPercent = 1.5f;
        */

        public void Allocate()
        {

        }


    }
}
