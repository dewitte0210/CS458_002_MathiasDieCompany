using System.Runtime.Serialization;

namespace FeatureRecognitionAPI.Models.Enums;

[DataContract]
public enum RuleType
{
    [EnumMember(Value = "2ptCB937")] TwoPtCB937,
    [EnumMember(Value = "2ptSB937")] TwoPtSB937,
    [EnumMember(Value = "2ptDDB937")] TwoPtDDB937,
    [EnumMember(Value = "2ptCB1125")] TwoPtCB1125,
    [EnumMember(Value = "3ptCB937")] ThreePtCB937,
    [EnumMember(Value = "3ptSB937")] ThreePtSB937,
    [EnumMember(Value = "3ptDDB937")] ThreePtDDB937,
    [EnumMember(Value = "3ptDSB927")] ThreePtDSB927,
    [EnumMember(Value = "412CB472")] FourTwelveCB472,
    [EnumMember(Value = "512CB472")] FiveTwelveCB472,
}