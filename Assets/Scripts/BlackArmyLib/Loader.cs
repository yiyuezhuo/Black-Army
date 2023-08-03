using System;
using System.Collections.Generic;
using YYZ.BlackArmy.Schema;
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

        /*
        public List<EdgeRow> edges;
        public List<HexRow> hexes;
        public List<DetachmentRow> detachments;
        public List<LeaderRow> leaders;
        public List<TraitStatsRow> traitStats;
        public List<ElementStatRow> elementStats;
        public List<LeaderAssignmentRow> leaderAssignments;
        public List<ElementAssignmentRow> elementAssignments;
        public List<ElementAttachmentRow> elementAttachments;
        public List<SideStatsRow> sides;
        public List<ElementCategoryRow> elementCategories;
        */

        /*
        public override string ToString()
        {
            return $"RawData(edges=[{edges.Count}], hexes=[{hexes.Count}], detachments=[{detachments.Count}], leaders=[{leaders.Count}], traitStats=[{traitStats.Count}], elementStats=[{elementStats.Count}], leaderAssignments=[{leaderAssignments.Count}], elementAssignments=[{elementAssignments.Count}], elementAttachments=[{elementAttachments.Count}], sides=[{sides.Count}], elementCategories=[{elementCategories.Count}])";
        }
        */

        /*
        public void Load()
        {
            edges = LoadList<EdgeRow>("Edges.csv");
            hexes = LoadList<HexRow>("Hexes.csv");
            detachments = LoadList<DetachmentRow>("Detachments.csv");
            leaders = LoadList<LeaderRow>("Leaders.csv");
            traitStats = LoadList<TraitStatsRow>("Trait Stats.csv");
            elementStats = LoadList<ElementStatRow>("Element Stats.csv");
            leaderAssignments = LoadList<LeaderAssignmentRow>("Leader Assignments.csv");
            elementAssignments = LoadList<ElementAssignmentRow>("Element Assignments.csv");
            elementAttachments = LoadList<ElementAttachmentRow>("Element Attachments.csv");
            sides = LoadList<SideStatsRow>("Side Stats.csv");
            elementCategories = LoadList<ElementCategoryRow>("Element Categories.csv");
        }

        public List<T> LoadList<T>(string name)
        {
            var bytes = reader.Read(name);
            var s = Encoding.UTF8.GetString(bytes);
            var memoryStream = new MemoryStream(bytes);

            using(var streamReader = new StreamReader(memoryStream))
            {
                
                using(var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<T>();
                    return records.ToList();
                }
            }
        }
        */

        public IEnumerable<T> ReadCsv<T>(string name, Func<CsvReader, T> f)
        {
            var bytes = reader.Read(name);
            var s = Encoding.UTF8.GetString(bytes);
            var memoryStream = new MemoryStream(bytes);

            using(var streamReader = new StreamReader(memoryStream))
            {
                using(var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while(csv.Read())
                    {
                        // csv.GetField<int>("a");
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

        /*
        Detachment ParseDetachment(CsvReader csv, Dictionary<string, Hex> hexMap) => new Detachment()
        {
            Name=csv.GetField<string>("ID"),
            Side=cs
        };
        */

        class TraitStats
        {
            public string Name;
            public int Strategic;
            public int Operational;
            public int Tactical;
            public int Guerrilla;
            public int Political;
        }

        public GameState GetGameState()
        {
            // First Pass: Barebone Allocation

            var elementCategories = ReadCsv("Element Categories.csv", ParseElementCategory).ToList();
            var elementCategoryMap = elementCategories.ToDictionary(d => d.Name);

            var hexMap = ReadCsv("Hexes.csv", ParseHex).ToDictionary(d => (d.X, d.Y));
            /*
            var elementCategoryMap = elementCategories.ToDictionary(row => row.Name, row => new ElementCategory() { Name = row.Name , Priority =row.Priority });

            var hexMap = hexes.ToDictionary(row => (row.X, row.Y), row => new Hex(){
                X=row.X, Y=row.Y, Type=row.Type
            });
            */

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
            /*

            foreach(var edgeRow in edges)
            {
                var src = hexMap[(edgeRow.SourceX, edgeRow.SourceY)];
                var dst = hexMap[(edgeRow.DestinationX, edgeRow.DestinationY)];
                src.EdgeMap[dst] = new Edge(){River=edgeRow.River, Railroad=edgeRow.Railroad, CountryBoundary=edgeRow.CountryBoundary};
            }
            */

            var gameSides = ReadCsv("Side Stats.csv", ParseSide).ToList();
            var sideMap = gameSides.ToDictionary(d => d.Name);

            /*
            var sideMap = sides.ToDictionary(row => row.ID, row => new Side(){
                Name=row.ID, Morale=row.Morale, VP=row.VP, RailroadMovementAvailable=row.Tags.Contains("Railroad Movement")
            });
            */

            var detachmentMap = ReadCsv("Detachments.csv", (csv) => new Detachment(){
                Name=csv.GetField<string>("ID"),
                Side=sideMap[csv.GetField<string>("Side")],
                Hex=hexMap[(csv.GetField<int>("X"), csv.GetField<int>("Y"))]
            }).ToDictionary(d => d.Name);

            /*
            var detachmentMap = detachments.ToDictionary(row => row.ID, row => new Detachment(){
                Name=row.ID, Side=sideMap[row.Side], Hex=hexMap[(row.X, row.Y)]
            });
            */

            // var traitMap = traitStats.ToDictionary(row => row.ID);

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

            /*
            var leaderMap = new Dictionary<string, Leader>();
            foreach(var row in leaders)
            {
                var leader = leaderMap[row.ID] = new Leader()
                {
                    Name=row.ID, Wiki=row.Wiki
                };
                if(row.MilitaryTrait != "")
                {
                    var trait = traitMap[row.MilitaryTrait];
                    ApplyTrait(leader, trait);
                }
            }
            */

            var elementTypes = ReadCsv("Element Stats.csv", (csv) => new ElementType(){
                Name=csv.GetField<string>("ID"),
                Category=elementCategoryMap[csv.GetField<string>("Category")],
                AllocationCoef=csv.GetField<float>("Allocation Coefficient"),
                Manpower=csv.GetField<int>("Manpower"),
                Speed=csv.GetField<float>("Speed"),
                TacticalSpeedModifier=csv.GetField<float>("Tactical Speed Modifier")
            });

            /*
            var elementTypes = elementStats.Select(row => new ElementType(){
                Name=row.ID, Category=elementCategoryMap[row.Category],
                AllocationCoef=row.AllocationCoefficient,
                Fire=row.Fire, Assault=row.Assault, Defense=row.Defense,
                // Morale=row.Morale,
                Manpower=row.Manpower, Speed=row.Speed, TacticalSpeedModifier=row.TacticalSpeedModifier
            });
            */

            var elementSystem = new ElementTypeSystem(elementTypes);

            foreach(var csv in ReadCsv("Element Attachments.csv"))
            {
                var attachment = elementSystem.GetType(csv.GetField<string>("Attachment"));
                var host = elementSystem.GetType(csv.GetField<string>("Host"));
                attachment.AttachCoefMap[host] = csv.GetField<float>("Coefficient");
            }

            /*
            foreach(var row in elementAttachments)
            {
                var attachment = elementSystem.GetType(row.Attachment);
                var host = elementSystem.GetType(row.Host);
                attachment.AttachCoefMap[host] = row.Coefficient;
            }
            */

            foreach(var csv in ReadCsv("Leader Assignments.csv"))
            {
                leaderMap[csv.GetField<string>("Leader")].Detachment = detachmentMap[csv.GetField<string>("Detachment")];
            }

            /*
            foreach(var row in leaderAssignments)
            {
                leaderMap[row.Leader].Detachment = detachmentMap[row.Detachment];
            }
            */

            foreach(var csv in ReadCsv("Element Assignments.csv"))
            {
                var elementType = elementSystem.GetType(csv.GetField<string>("Element Type"));
                var detachment = detachmentMap[csv.GetField<string>("Detachment")];
                detachment.Elements.Add(elementType, csv.GetField<int>("Strength"));
            }

            /*
            foreach(var row in elementAssignments)
            {
                var elementType = elementSystem.GetType(row.ElementType);
                var detachment = detachmentMap[row.Detachment];
                detachment.Elements.Add(elementType, row.Strength);
            }
            */

            // var gameSides = ReadCsv("Side Stats.csv", (csv) => )

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

            /*
            foreach(var row in sides)
            {
                var side = sideMap[row.ID];
                side.PlaceholderLeader = new Leader()
                {
                    Name=row.PlaceholderLeaderName,
                    IsPlaceholdLeader=true
                };
                var trait = traitMap[row.PlaceholderLeaderTrait];
                ApplyTrait(side.PlaceholderLeader, trait);
            }
            */
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

            // Create GameState

            // var gameSides = sides.Select(row=>sideMap[row.ID]).ToList(); // Keep Order
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
