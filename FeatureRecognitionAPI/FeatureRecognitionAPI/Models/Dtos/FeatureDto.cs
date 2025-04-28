using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FeatureRecognitionAPI.Models.Dtos
{
    public class Features
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PossibleFeatureTypes FeatureType { get; set; }

        // At least some properties will not be null, depending on the entity type
        public List<EntityDto>? EntityList { get; set; }
        public int Count { get; set; }
        public double Perimeter { get; set; }
        
        public double Diameter { get; set; }
        public int MultipleRadius { get; set; }
        public bool KissCut { get; set; }
    }

    public class DieDto
    {
        public required List<Features> Features { get; set; }

        [JsonProperty(PropertyName = "Count")]
        // Number Up
        public int NumberUp { get; set; } 

    }
}
