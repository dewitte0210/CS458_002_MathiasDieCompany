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

    [JsonProperty] private readonly double minX;
    [JsonProperty] private readonly double minY;
    [JsonProperty] private readonly double maxX;
    [JsonProperty] private readonly double maxY;

    public JsonPackage(List<List<Entity>> touchingEntitiesList, List<FeatureGroup> featureGroups)
    {
        _touchingEntitiesList = touchingEntitiesList;
        _featureGroups = featureGroups;

        var tempMinX = Double.MaxValue;
        var tempMinY = Double.MaxValue;
        
        var tempMaxX = Double.MinValue;
        var tempMaxY = Double.MinValue;

        foreach (List<Entity> list in touchingEntitiesList)
        {
            foreach (Entity item in list)
            {
                tempMinX = Math.Min(tempMinX, item.MinX());
                tempMinY = Math.Min(tempMinY, item.MinY());
                tempMaxX = Math.Max(tempMaxX, item.MaxX());
                tempMaxY = Math.Max(tempMaxY, item.MaxY());
            } 
        }

        minX = tempMinX;
        minY = tempMinY;
        maxX = tempMaxX;
        maxY = tempMaxY;
    }

}