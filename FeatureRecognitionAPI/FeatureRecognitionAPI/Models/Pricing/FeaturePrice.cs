using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models.Pricing;

public class FeaturePrice(PossibleFeatureTypes type, double setupRate, double runRate, double difficultyFactor, int maxRadius, int quantity)
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
    /// Denotes the Part Quantity used to make the feature
    /// </summary>
    public int Quantity { get; set; } = quantity;

    public int MaxRadius { get; set; } = maxRadius; 
    
    public PossibleFeatureTypes Type { get; set; } = type;
}