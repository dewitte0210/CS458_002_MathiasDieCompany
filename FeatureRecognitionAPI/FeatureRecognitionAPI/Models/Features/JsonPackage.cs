using FeatureRecognitionAPI.Models.Entities;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Features;

/// <summary>
/// Helper class to send both table data and draw data to the front end.
/// </summary>
public class JsonPackage
{
    [JsonProperty] private readonly List<Entity> _touchingEntitiesList;

    [JsonProperty] private readonly List<FeatureGroup> _featureGroups;

    [JsonProperty] private readonly double minX;
    [JsonProperty] private readonly double minY;
    [JsonProperty] private readonly double maxX;
    [JsonProperty] private readonly double maxY;

    public JsonPackage(List<Entity> touchingEntitiesList, List<FeatureGroup> featureGroups)
    {
        _touchingEntitiesList = touchingEntitiesList;
        _featureGroups = featureGroups;

        var tempMinX = Double.MaxValue;
        var tempMinY = Double.MaxValue;

        var tempMaxX = Double.MinValue;
        var tempMaxY = Double.MinValue;

        foreach (Entity item in touchingEntitiesList)
        {
            tempMinX = Math.Min(tempMinX, item.MinX());
            tempMinY = Math.Min(tempMinY, item.MinY());
            tempMaxX = Math.Max(tempMaxX, item.MaxX());
            tempMaxY = Math.Max(tempMaxY, item.MaxY());
        }
        

        minX = tempMinX;
        minY = tempMinY;
        maxX = tempMaxX;
        maxY = tempMaxY;
    }
}