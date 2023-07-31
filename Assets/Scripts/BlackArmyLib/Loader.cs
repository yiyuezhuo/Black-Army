using System;
using System.Collections.Generic;
using YYZ.BlackArmy.Schema;
using System.IO;
using System.Globalization;
using System.Text;
using System.Linq;

using YYZ.BlackArmy.Model;

namespace YYZ.BlackArmy.Loader
{
    public interface ITableReader
    {
        public byte[] Read(string name);
    }

    public class RawData
    {
        public ITableReader reader;

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

        public override string ToString()
        {
            return $"RawData(edges=[{edges.Count}], hexes=[{hexes.Count}], detachments=[{detachments.Count}], leaders=[{leaders.Count}], traitStats=[{traitStats.Count}], elementStats=[{elementStats.Count}], leaderAssignments=[{leaderAssignments.Count}], elementAssignments=[{elementAssignments.Count}], elementAttachments=[{elementAttachments.Count}], sides=[{sides.Count}], elementCategories=[{elementCategories.Count}])";
        }

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
                
                using(var csv = new CsvHelper.CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<T>();
                    return records.ToList();
                }
            }
        }

        void ApplyTrait(Leader leader, TraitStatsRow row)
        {
            leader.Strategic = row.Strategic;
            leader.Operational = row.Operational;
            leader.Tactical = row.Tactical;
            leader.Guerrilla = row.Guerrilla;
            leader.Political = row.Political;
        }

        public GameState GetGameState()
        {
            // First Pass: Barebone Allocation

            var elementCategoryMap = elementCategories.ToDictionary(row => row.Name, row => new ElementCategory() { Name = row.Name , Priority =row.Priority });

            var hexMap = hexes.ToDictionary(row => (row.X, row.Y), row => new Hex(){
                X=row.X, Y=row.Y, Type=row.Type
            });

            foreach(var edgeRow in edges)
            {
                var src = hexMap[(edgeRow.SourceX, edgeRow.SourceY)];
                var dst = hexMap[(edgeRow.DestinationX, edgeRow.DestinationY)];
                src.EdgeMap[dst] = new Edge(){River=edgeRow.River, Railroad=edgeRow.Railroad, CountryBoundary=edgeRow.CountryBoundary};
            }

            var sideMap = sides.ToDictionary(row => row.ID, row => new Side(){
                Name=row.ID, Morale=row.Morale, VP=row.VP
            });

            var detachmentMap = detachments.ToDictionary(row => row.ID, row => new Detachment(){
                Name=row.ID, Side=sideMap[row.Side], Hex=hexMap[(row.X, row.Y)]
            });

            var traitMap = traitStats.ToDictionary(row => row.ID);
            
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

            var elementTypes = elementStats.Select(row => new ElementType(){
                Name=row.ID, Category=elementCategoryMap[row.Category],
                AllocationCoef=row.AllocationCoefficient,
                Fire=row.Fire, Assault=row.Assault, Defense=row.Defense,
                // Morale=row.Morale,
                Manpower=row.Manpower, Speed=row.Speed, TacticalSpeedModifier=row.TacticalSpeedModifier
            });

            var elementSystem = new ElementTypeSystem(elementTypes);

            foreach(var row in elementAttachments)
            {
                var attachment = elementSystem.GetType(row.Attachment);
                var host = elementSystem.GetType(row.Host);
                attachment.AttachCoefMap[host] = row.Coefficient;
            }

            foreach(var row in leaderAssignments)
            {
                leaderMap[row.Leader].Detachment = detachmentMap[row.Detachment];
            }

            foreach(var row in elementAssignments)
            {
                var elementType = elementSystem.GetType(row.ElementType);
                var detachment = detachmentMap[row.Detachment];
                detachment.Elements.Add(elementType, row.Strength);
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

            // Create GameState

            var gameSides = sides.Select(row=>sideMap[row.ID]).ToList(); // Keep Order
            return new GameState()
            {
                Sides=gameSides, CurrentSide=gameSides[0],
                ElementCategories=elementCategories.Select(row => elementCategoryMap[row.Name]).ToList(),
                ElementTypeSystem=elementSystem
            };
        }
    }
}
