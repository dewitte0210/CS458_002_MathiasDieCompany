namespace FeatureRecognitionAPI.Models.Pricing;

public class PunchPrice
{
    public double CutSize {get; set;}
    public double BaseSize { get; set; }  
    
    // Marked as Cost in the VB6 app - Renamed for improved readability matching Riley's names
    public double SetupCost { get; set; }   
    
    // Marked as Install in the VB6 app
    public double RunCost{ get; set; }

    public PunchPrice() { }

    public PunchPrice(double cutSize, double baseSize, double cost, double installCharge)
    {
        CutSize = cutSize;
        BaseSize = baseSize;
        SetupCost = cost;
        RunCost = installCharge;
    }
}