using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Models.Dtos;

public class PunchPriceReturn
{

    public List<PunchPrice> TubePunchList { get; set; }
    public List<PunchPrice> SoPunchList { get; set; }
    public List<PunchPrice> HdsoPunchList { get; set; }
    public List<PunchPrice> FtPunchList{ get; set; }
    public List<PunchPrice> SwPunchList { get; set; }
    public List<PunchPrice> RetractList { get; set; }
}