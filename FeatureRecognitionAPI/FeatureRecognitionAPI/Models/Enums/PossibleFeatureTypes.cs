using System.Runtime.Serialization;

namespace FeatureRecognitionAPI.Models.Enums
{
    public enum PossibleFeatureTypes
    {
        [EnumMember(Value = "Unknown")]
        Unknown,
        [EnumMember(Value = "SideTubePunch")]
        StdTubePunch,
        SideOutlet,
        HDSideOutlet,
        StdFTPunch,
        StdSWPunch,
        StdRetractPins,
        Punch,
        Group1A1, // Metered Rectangle
        Group1A2, // Radius Corner Rectangle
        Group1B1, // Circle
        Group1B2, // Rounded Rectangle
        Group3,
        Group4,
        Group5,
        Group1C, // Triangles
        Group6,
        Group2A1, // Elipses
        Group2A2, // Bowties
        Group7,
        Group8,
        Group9,
        Group10,
        Group11,
        Group12,
        Group13,
        Group14,
        Group17,
        GroupS1,
        GroupS2,
        GroupS3,
        GroupS4
    }
    // Group 15 and 16 don't seem to have prices
}
