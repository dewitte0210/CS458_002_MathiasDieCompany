using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models.Pricing;

public class FeaturePrice(PossibleFeatureTypes type, double setupRate, double runRate, double difficultyFactor, int quantity )
{
    /// <summary>
    ///  Denotes the hour/part rate of setting up the cut
    /// </summary>
    public double SetupRate { get; set; } = setupRate;

    /// <summary>
    /// Denotes the hour/part rate of running the cut
    /// </summary>
    public double RunRate { get; set; } = runRate;

    public double DifficultyFactor { get; set; } = difficultyFactor;

    /// <summary>
    /// Not entirely sure what quantity this refers to yet as the VB6 logic is using different numbers for this
    /// even when there is only one of the parts
    /// </summary>
    public int Quantity { get; set; } = quantity;

    public PossibleFeatureTypes Type { get; set; } = type;
}