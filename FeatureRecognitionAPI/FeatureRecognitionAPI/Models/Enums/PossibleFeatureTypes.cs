using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Enums
{
    public enum PossibleFeatureTypes
    {
        [JsonProperty]
        Unknown,
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
        Group2A2 // Bowties
    }
}
