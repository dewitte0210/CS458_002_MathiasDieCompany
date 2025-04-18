using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FeatureRecognitionAPI.Models.Dtos
{
    public class QuoteSubmissionDto
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleType RuleType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EjectionMethodType EjectionMethod { get; set; }
        public List<DieDto> FeatureGroups { get; set; }

    }
}
