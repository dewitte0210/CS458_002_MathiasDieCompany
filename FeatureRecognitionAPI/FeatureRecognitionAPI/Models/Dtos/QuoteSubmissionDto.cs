using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FeatureRecognitionAPI.Models.Dtos
{
    public class QuoteSubmissionDto
    {
        //TODO: ADD RuleType Enum
        public string RuleType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EjectionMethodTypeEnum EjectionMethod { get; set; }
        public List<FeatureDto> Features { get; set; }
    }
}
