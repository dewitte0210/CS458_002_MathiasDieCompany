using FeatureRecognitionAPI.Models.Enums;
namespace FeatureRecognitionAPI.Models.Pricing;

public class PunchPrice
{
    public PunchPrice()
    {
    }

    public PunchPrice(double cutSize, double baseSize, double cost, double installCharge)
    {
        CutSize = cutSize;
        BaseSize = baseSize;
        SetupCost = cost;
        RunCost = installCharge;
    }

    public double CutSize {get; set;} 
    public double BaseSize { get; set; } 
    public double SetupCost { get; set; }
    public double RunCost{ get; set; }

}