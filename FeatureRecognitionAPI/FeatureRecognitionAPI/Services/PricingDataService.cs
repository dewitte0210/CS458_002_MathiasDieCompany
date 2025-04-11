using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Pricing;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Services;

public class PricingDataService : IPricingDataService
{
    private readonly string FEATURE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "FeaturePrices.json");
    private readonly string TUBE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "TubePunchPrices.json");
    private readonly string SO_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "SoPunchPrices.json");
    private readonly string HDSO_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "HdsoPunchPrices.json");
    private readonly string FT_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "FtPunchPrices.json");
    private readonly string SW_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "SwPunchPrices.json");
    private readonly string RETRACT_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData", "RetractPrices.json");
    public List<PunchPrice> _tubePunchList { get; set; }
    public List<PunchPrice> _soPunchList { get; set; }
    public List<PunchPrice> _hdsoPunchList { get; set; }
    public List<PunchPrice> _ftPunchList { get; set; }
    public List<PunchPrice> _swPunchList { get; set; }
    public List<PunchPrice> _retractList { get; set; }
    private readonly List<FeaturePrice> _featurePriceList;

    public PricingDataService()
    {
        var featurePriceReader = new StreamReader(FEATURE_PATH);
        var tubePunchReader = new StreamReader(TUBE_PATH);
        var soPunchReader = new StreamReader(SO_PATH);
        var hdsoPunchReader = new StreamReader(HDSO_PATH);
        var ftPunchReader = new StreamReader(FT_PATH);
        var swPunchReader = new StreamReader(SW_PATH);
        var retractReader = new StreamReader(RETRACT_PATH);
       
        string featureJson = featurePriceReader.ReadToEnd();
        string tubePunchJson = tubePunchReader.ReadToEnd();
        string soPunchJson = soPunchReader.ReadToEnd();
        string hdsoPunchJson = hdsoPunchReader.ReadToEnd();
        string ftPunchJson = ftPunchReader.ReadToEnd();
        string swPunchJson = swPunchReader.ReadToEnd();
        string retractJson = retractReader.ReadToEnd();
        
        _featurePriceList = JsonConvert.DeserializeObject<List<FeaturePrice>>(featureJson);
        _tubePunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(tubePunchJson); 
        _soPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(soPunchJson);
        _hdsoPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(hdsoPunchJson);
        _ftPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(ftPunchJson);
        _swPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(swPunchJson);
        _retractList = JsonConvert.DeserializeObject<List<PunchPrice>>(retractJson);
        
        featurePriceReader.Close();
        tubePunchReader.Close();
        soPunchReader.Close();
        hdsoPunchReader.Close();
        ftPunchReader.Close();
        swPunchReader.Close();
        retractReader.Close();
    }
    
    public List<FeaturePrice> GetFeaturePrices(){ return _featurePriceList; }

    public PunchPriceReturn GetPunchPrices()
    {
        return new PunchPriceReturn()
        {
            TubePunchList = _tubePunchList,
            SoPunchList = _soPunchList,
            HdsoPunchList = _hdsoPunchList,
            FtPunchList = _ftPunchList,
            SwPunchList = _swPunchList,
            RetractList = _retractList,
        };
    }
    
    public bool UpdatePunchPrice(PossibleFeatureTypes type, List<PunchPrice> prices)
    {
        return false;
    }

    public bool UpdateFeaturePrice(List<FeaturePrice> prices)
    {
        return false;
    }

    public bool deletePunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice)
    {
        return false;
    }

    public bool AddPunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice)
    {
        return false;
    }

    public bool UpdatePunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice)
    {
        return false;
    }

    public bool UpdateFeaturePrice(PossibleFeatureTypes type, FeaturePrice featurePrice)
    {
        return false;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool WriteToDBFile(string path, Object obj)
    {   
        StreamReader sr = new StreamReader(path);
        string jsonCopy = sr.ReadToEnd();
          
        return true;
    }
}