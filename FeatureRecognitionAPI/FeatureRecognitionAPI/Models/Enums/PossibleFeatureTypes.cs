using System.Runtime.Serialization;

namespace FeatureRecognitionAPI.Models.Enums
{
    public enum PossibleFeatureTypes
    {
        [EnumMember(Value = "SideTubePunch")] StdTubePunch,
        [EnumMember(Value = "Unknown")] Unknown,
        HDSideOutlet,
        Punch,
        SideOutlet,
        StdFTPunch,
        StdRetractPins,
        StdSWPunch,
        Group1A1, // Metered Rectangle.
        Group1A2, // Radius Corner Rectangle.
        Group1B1, // Circle.
        Group1B2, // Rounded Rectangle.
        Group1C, // Triangles.
        Group2A1, // Elipses.
        Group2A2, // Bowties.
        Group3,
        Group4,
        Group5,
        Group6,
        Group7,
        Group8,
        Group9,
        Group10,
        Group11,
        Group12a, // Double D.
        Group12b, // Cross-Hairs.
        Group13,
        Group14,
        Group17,
        GroupS1,
        GroupS2,
        GroupS3,
        GroupS4
    }
    // Group 15 and 16 don't seem to have prices.
}
