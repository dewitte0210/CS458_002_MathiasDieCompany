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

    public static double PunchDiscount(PossibleFeatureTypes punchType, int count)
    {
        return punchType switch
        {
            PossibleFeatureTypes.HDSideOutlet => HDSideDiscount(count),
            PossibleFeatureTypes.StdFTPunch => StdFtDiscount(count),
            PossibleFeatureTypes.StdSWPunch => StdSwDiscount(count),
            PossibleFeatureTypes.StdTubePunch => TubeDiscount(count),
            PossibleFeatureTypes.SideOutlet => SideOutletDiscount(count),
            _ => 1
        };
    }

    private static double SideOutletDiscount(int count)
    {
        return count switch
        {
            >= 200 => 0.73,
            >= 100 => 0.77,
            >= 50 => 0.82,
            >= 25 => 0.90,
            >= 13 => 0.94,
            _ => 1
        };
    }
    
    private static double TubeDiscount(int count)
    {
        return count switch
        {
            >= 200 => 0.56,
            >= 100 => 0.64,
            >= 50 => 0.73,
            >= 25 => 0.85,
            >= 13 => 0.93,
            _ => 1
        };
    }
    private static double StdSwDiscount(int count)
    {
        return count switch
        {
            >= 50 => 0.79,
            >= 25 => 0.86,
            >= 13 => 0.93,
            _ => 1
        };
    }
    private static double StdFtDiscount(int count)
    {
        return count switch
        {
            >= 200 => 0.67,
            >= 100 => 0.78,
            >= 50 => 0.82,
            >= 25 => 0.86,
            >= 13 => 0.93,
            _ => 1
        };
    }
    private static double HDSideDiscount(int count)
    {
        return  count switch
        {
            >= 200 => 0.83,
            >= 100 => 0.86,
            >= 50 => 0.89,
            >= 25 => 0.93,
            >= 13 => 0.97,
            _ => 1
        };
    }
}