using System.Runtime.Serialization;

namespace FeatureRecognitionAPI.Models.Enums
{
    public enum PossibleFeatureTypes
    {
        [EnumMember(Value = "Unknown")]
        Unknown, 
        [EnumMember(Value = "SideTubePunch")]
        StdTubePunch, // Tube punch - Auto
        SideOutlet, // Auto
        HDSideOutlet, // Heavy Duty - Auto
        StdFTPunch, // Feed through - Auto
        StdSWPunch, // Side Wall - Auto
        StdRetractPins, // Auto
        Punch, // Auto
        Group1A1, // Metered Rectangle - Auto
        Group1A2, // Radius Corner Rectangle - Auto
        Group1B1, // Circle - Auto
        Group1B2, // Rounded Rectangle - Auto
        Group3, // Chamfered corners - Auto
        Group4, // Corner notch - Auto
        Group5, //  Mitered Notch - Auto
        Group1C, // Triangles - Auto
        Group6, // radius notch - Auto
        Group2A1, // Elipses - Auto
        Group2A2, // Bowties - Auto
        Group7, // Small Obrounds - Manual
        Group8, // D-sub Connector - Manual
        Group9, // Kiss cut tab - Auto
        Group10, // C-Spacer - Auto
        Group11, // Pac-Man - Auto
        Group12a, // Double D - Auto
        Group12b, // Cross-Hairs - Auto
        Group13, // Circuit tails - Manual
        Group14, // Knifed Cavities - Manual
        Group17, // Radius Notch - Auto
        GroupS1, // Vent - Manual
        GroupS2, // Vent - Manual
        GroupS3, // Vent - Manual
        GroupS4 // Vent - Manual
    }
    // Group 15 and 16 don't seem to have prices
}
