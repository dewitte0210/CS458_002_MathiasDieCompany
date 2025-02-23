using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Features;

/**
 * Helper class to send both table data and draw data to the front end.
 */
public class JsonPackage
{
    [JsonProperty]
    private readonly List<List<Entity>> _touchingEntitiesList;

    [JsonProperty]
    private readonly List<FeatureGroup> _featureGroups;

    public JsonPackage(List<List<Entity>> touchingEntitiesList, List<FeatureGroup> featureGroups)
    {
        _touchingEntitiesList = touchingEntitiesList;
        _featureGroups = featureGroups;
    }

}