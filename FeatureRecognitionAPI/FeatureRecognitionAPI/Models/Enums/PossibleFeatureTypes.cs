using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Enums
{
    public enum PossibleFeatureTypes
    {
        StdTubePunch,
        SideOutletPunch,
        HDSideOutletPunch,
        StdFeedThroughPunch,
        StdStraightWallPunch,
        StdRetractablePinPunch,
        Group1A1,
        Group1A2,
        Group1B1, // Circle
        Group1B2, // Rounded Rectangle
        Group3,
        Group1C,
        Group6,
        Group2A
    }
}
