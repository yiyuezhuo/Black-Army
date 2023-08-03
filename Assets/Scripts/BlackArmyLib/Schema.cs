// Obsolete
namespace YYZ.BlackArmy.Schema
{
    using CsvHelper.Configuration.Attributes;

    public class EdgeRow
    {
        // [Name("Sourc")]
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public int DestinationX { get; set; }
        public int DestinationY { get; set; }
        public bool River { get; set; }
        public bool Railroad { get; set; }
        public bool CountryBoundary { get; set; }
    }

    public class HexRow
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; }
    }

    public class DetachmentRow
    {
        public string ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Side { get; set; }
    }

    public class LeaderRow
    {
        public string ID{get;set;}

        [Name("Full Name")]
        public string FullName{get;set;}
        public string Side{get;set;}
        public string Location{get;set;}

        [Name("Military Trait")]
        public string MilitaryTrait{get;set;}
        public string Wiki{get;set;}
    }

    public class TraitStatsRow
    {
        public string ID{get;set;}
        public int Strategic{get;set;}
        public int Operational{get;set;}
        public int Tactical{get;set;}
        public int Guerrilla{get;set;}
        public int Political{get;set;}
    }

    public class ElementStatRow
    {
        public string ID{get;set;}
        public string Category { get; set; }
        public string Side{get;set;}
        public float Speed{get;set;}

        [Name("Tactical Speed Modifier")]
        public float TacticalSpeedModifier{get;set;}
        public float Fire{get;set;}
        public float Assault{get;set;}
        public float Defense{get;set;}
        public string Morale{get;set;}
        public int Manpower{get;set;}

        [Name("Allocation Coefficient")]
        public float AllocationCoefficient{get;set;}
    }

    public class LeaderAssignmentRow
    {
        public string Leader{get;set;}
        public string Detachment{get;set;}
    }

    public class ElementAssignmentRow
    {
        public string Detachment{get;set;}

        [Name("Element Type")]
        public string ElementType{get;set;}
        public int Strength{get;set;}
    }

    public class ElementAttachmentRow
    {
        public string Attachment{get;set;}
        public string Host{get;set;}
        public float Coefficient{get;set;}
    }

    public class SideStatsRow
    {
        public string ID{get;set;}
        public float Morale{get;set;}
        public float VP{get;set;}

        [Name("Placeholder Leader Name")]
        public string PlaceholderLeaderName{get;set;}

        [Name("Placeholder Leader Trait")]
        public string PlaceholderLeaderTrait{get;set;}
        public string Tags { get; set; }
    }

    public class ElementCategoryRow
    {
        public string Name { get; set; }
        public string Tags { get; set; }
        public int Priority { get; set; }
    }
}