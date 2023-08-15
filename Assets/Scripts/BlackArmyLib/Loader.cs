using System;
using System.Collections.Generic;
// using YYZ.BlackArmy.Schema; // Obsolete
using System.IO;
using System.Globalization;
using System.Text;
using System.Linq;

using YYZ.BlackArmy.Model;
using CsvHelper;

namespace YYZ.BlackArmy.Loader
{
    public interface ITableReader
    {
        public byte[] Read(string name);
    }

    public class RawData
    {
        public ITableReader reader;

        public IEnumerable<T> ReadCsv<T>(string name, Func<CsvReader, T> f)
        {
            var bytes = reader.Read(name);
            // var s = Encoding.UTF8.GetString(bytes);
            var memoryStream = new MemoryStream(bytes);

            using(var streamReader = new StreamReader(memoryStream))
            {
                using(var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while(csv.Read())
                    {
                        yield return f(csv);
                    }
                }
            }
        }

        IEnumerable<CsvReader> ReadCsv(string name) => ReadCsv(name, d => d);

        void ApplyTrait(Leader leader, TraitStats row)
        {
            leader.Strategic = row.Strategic;
            leader.Operational = row.Operational;
            leader.Tactical = row.Tactical;
            leader.Guerrilla = row.Guerrilla;
            leader.Political = row.Political;
        }

        // First Pass Parsing Initialization Constructors
        ElementCategory ParseElementCategory(CsvReader csv) => new ElementCategory() 
        {
            Name=csv.GetField<string>("Name"),
            Priority=csv.GetField<int>("Priority")
        };

        Hex ParseHex(CsvReader csv) => new Hex()
        {
            X=csv.GetField<int>("X"),
            Y=csv.GetField<int>("Y"),
            Type=csv.GetField<string>("Type")
        };

        Side ParseSide(CsvReader csv) => new Side()
        {
            Name=csv.GetField<string>("ID"),
            Morale=csv.GetField<float>("Morale"),
            VP=csv.GetField<float>("VP"),
            RailroadMovementAvailable=csv.GetField<string>("Tags").Contains("Railroad Movement"),
            PlaceholderLeaderName=csv.GetField<string>("Placeholder Leader Name"),
            PlaceholderLeaderTrait=csv.GetField<string>("Placeholder Leader Trait"),
        };

        class TraitStats
        {
            public string Name;
            public int Strategic;
            public int Operational;
            public int Tactical;
            public int Guerrilla;
            public int Political;
        }

        static Dictionary<string, int> moraleCodeMap = new()
        {
            {"A", 6},
            {"B", 5},
            {"C", 4},
            {"D", 3},
            {"E", 2},
            {"F", 1}
        };

        public GameState GetGameState()
        {
            // First Pass: Barebone Allocation

            var elementCategories = ReadCsv("Element Categories.csv", ParseElementCategory).ToList();
            var elementCategoryMap = elementCategories.ToDictionary(d => d.Name);

            var hexMap = ReadCsv("Hexes.csv", ParseHex).ToDictionary(d => (d.X, d.Y));

            foreach(var csv in ReadCsv("Edges.csv"))
            {
                var src = hexMap[(csv.GetField<int>("SourceX"), csv.GetField<int>("SourceY"))];
                var dst = hexMap[(csv.GetField<int>("DestinationX"), csv.GetField<int>("DestinationY"))];
                src.EdgeMap[dst] = new Edge()
                {
                    River=csv.GetField<bool>("River"),
                    Railroad=csv.GetField<bool>("Railroad"),
                    CountryBoundary=csv.GetField<bool>("CountryBoundary")
                };
            }

            var gameSides = ReadCsv("Side Stats.csv", ParseSide).ToList();
            var sideMap = gameSides.ToDictionary(d => d.Name);

            var detachmentMap = ReadCsv("Detachments.csv", (csv) => new Detachment(){
                Name=csv.GetField<string>("ID"),
                Side=sideMap[csv.GetField<string>("Side")],
                Hex=hexMap[(csv.GetField<int>("X"), csv.GetField<int>("Y"))],
                RuleOfEngagement=GameState.RoEList[0]
            }).ToDictionary(d => d.Name);

            var traitMap = ReadCsv("Trait Stats.csv", (csv) => new TraitStats(){
                Name=csv.GetField<string>("ID"),
                Strategic=csv.GetField<int>("Strategic"),
                Operational=csv.GetField<int>("Operational"),
                Tactical=csv.GetField<int>("Tactical"),
                Guerrilla=csv.GetField<int>("Guerrilla"),
                Political=csv.GetField<int>("Political")
            }).ToDictionary(d => d.Name);

            var leaderMap = ReadCsv("Leaders.csv", (csv) => new Leader(){
                Name=csv.GetField<string>("ID"),
                Wiki=csv.GetField<string>("Wiki"),
                Trait=csv.GetField<string>("Military Trait")
            }).ToDictionary(d => d.Name);

            foreach(var leader in leaderMap.Values)
                if(leader.Trait != "")
                    ApplyTrait(leader, traitMap[leader.Trait]);

            var elementTypes = ReadCsv("Element Stats.csv", (csv) => new ElementType(){
                Name=csv.GetField<string>("ID"),
                Category=elementCategoryMap[csv.GetField<string>("Category")],
                AllocationCoef=csv.GetField<float>("Allocation Coefficient"),
                FireSoft=csv.GetField<float>("Fire Soft"),
                FireHard=csv.GetField<float>("Fire Hard"),
                // Fire=csv.GetField<float>("Fire"),
                // Assault=csv.GetField<float>("Assault"),
                AssaultAttack=csv.GetField<float>("Assault Attack"),
                AssaultDefense=csv.GetField<float>("Assault Defense"),
                Width=csv.GetField<float>("Width"),
                ArmorValue=csv.GetField<float>("Armor Value"),
                Defense=csv.GetField<float>("Defense"),
                Morale=moraleCodeMap[csv.GetField<string>("Morale")],
                Manpower=csv.GetField<int>("Manpower"),
                Speed=csv.GetField<float>("Speed"),
                TacticalSpeedModifier=csv.GetField<float>("Tactical Speed Modifier")
            });

            var elementSystem = new ElementTypeSystem(elementTypes);

            foreach(var csv in ReadCsv("Element Attachments.csv"))
            {
                var attachment = elementSystem.GetType(csv.GetField<string>("Attachment"));
                var host = elementSystem.GetType(csv.GetField<string>("Host"));
                attachment.AttachCoefMap[host] = csv.GetField<float>("Coefficient");
            }

            foreach(var csv in ReadCsv("Leader Assignments.csv"))
            {
                leaderMap[csv.GetField<string>("Leader")].Detachment = detachmentMap[csv.GetField<string>("Detachment")];
            }

            foreach(var csv in ReadCsv("Element Assignments.csv"))
            {
                var elementType = elementSystem.GetType(csv.GetField<string>("Element Type"));
                var detachment = detachmentMap[csv.GetField<string>("Detachment")];
                detachment.Elements.Add(elementType, csv.GetField<int>("Strength"));
            }

            // Second Pass: Create Connection

            foreach(var detachment in detachmentMap.Values)
            {
                detachment.Side.Detachments.Add(detachment);
                detachment.Hex.Detachments.Add(detachment);
            }

            foreach(var leader in leaderMap.Values)
            {
                if(leader.Detachment != null)
                    leader.Detachment.Leaders.Add(leader);
            }

            foreach(var side in gameSides)
            {
                side.PlaceholderLeader = new Leader()
                {
                    Name=side.PlaceholderLeaderName,
                    IsPlaceholdLeader=true
                };
                var trait = traitMap[side.PlaceholderLeaderTrait];
                ApplyTrait(side.PlaceholderLeader, trait);
            }

            foreach(var hex in hexMap.Values)
                foreach(var side in gameSides)
                    hex.SideValueMap[side] = new();

            // Create GameState

            return new GameState()
            {
                Sides=gameSides, CurrentSide=gameSides[0],
                Hexes=hexMap.Values.ToList(),
                ElementCategories=elementCategories,
                ElementTypeSystem=elementSystem
            };
        }
    }
}
