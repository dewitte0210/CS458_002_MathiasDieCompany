using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Enums
{
    public enum PossibleFeatureTypes
    {
        [JsonProperty]
        SideTubePunch,
        SideOutlet,
        HDSideOutlet,
        StdFTPunch,
        StdSWPunch,
        StdRetractPins,
        Punch,
        Group1A1,
        Group1A2,
        Group1B1, // Circle
        Group1B2, // Rounded Rectangle
        Group3,
        Group4,
        Group5,
        Group1C,
        Group6,
        Group2A
    }
}
