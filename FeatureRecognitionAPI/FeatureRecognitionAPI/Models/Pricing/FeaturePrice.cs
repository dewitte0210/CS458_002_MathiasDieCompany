using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Models.Pricing;

public class FeaturePrice
{
    public FeaturePrice()
    {
    }

    public FeaturePrice(PossibleFeatureTypes type, double setupRate, double runRate, double difficultyFactor, int maxRadius, int quantity)
    {
        SetupRate = setupRate;
        RunRate = runRate;
        DifficultyFactor = difficultyFactor;
        MaxRadius = maxRadius;
        Quantity = quantity;
        Type = type;
    }
    /// <summary>
    ///  Denotes the hour/part rate of setting up the cut.
    /// </summary>
    public double SetupRate { get; set; }

    /// <summary>
    /// Denotes the hour/part rate of running the cut.
    /// </summary>
    public double RunRate { get; set; }

    public double DifficultyFactor { get; set; }

    /// <summary>
    /// Denotes the Part Quantity used to make the feature.
    /// </summary>
    public int Quantity { get; set; }
    public int MaxRadius { get; set; } 
    public PossibleFeatureTypes Type { get; set; }
}