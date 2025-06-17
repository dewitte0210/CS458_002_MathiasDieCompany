using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Pricing;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Services;

/// <summary>
/// Service class to perform CRUD operations on Pricing data.
/// </summary>
public class PricingDataService : IPricingDataService
{
    private static readonly string BASE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "PricingData");
    private readonly string FEATURE_PATH = Path.Combine(BASE_PATH, "FeaturePrices.json");
    private readonly string TUBE_PATH = Path.Combine(BASE_PATH, "TubePunchPrices.json");
    private readonly string SO_PATH = Path.Combine(BASE_PATH, "SoPunchPrices.json");
    private readonly string HDSO_PATH = Path.Combine(BASE_PATH, "HdsoPunchPrices.json");
    private readonly string FT_PATH = Path.Combine(BASE_PATH, "FtPunchPrices.json");
    private readonly string SW_PATH = Path.Combine(BASE_PATH, "SwPunchPrices.json");
    private readonly string RETRACT_PATH = Path.Combine(BASE_PATH, "RetractPrices.json");
    private readonly string RATES_PATH = Path.Combine(BASE_PATH, "BasePrices.json");
    public List<PunchPrice> _tubePunchList { get; set; }
    public List<PunchPrice> _soPunchList { get; set; }
    public List<PunchPrice> _hdsoPunchList { get; set; }
    public List<PunchPrice> _ftPunchList { get; set; }
    public List<PunchPrice> _swPunchList { get; set; }
    public List<PunchPrice> _retractList { get; set; }
    public List<FeaturePrice> _featurePriceList { get; set; }

    private RatesPrices _ratesPrices { get; set; }

    /// <summary>
    /// Default Constructor reads all of the pricing data from their respective file in the JSON "Database".
    /// </summary>
    public PricingDataService()
    {
        var featurePriceReader = new StreamReader(FEATURE_PATH);
        var tubePunchReader = new StreamReader(TUBE_PATH);
        var soPunchReader = new StreamReader(SO_PATH);
        var hdsoPunchReader = new StreamReader(HDSO_PATH);
        var ftPunchReader = new StreamReader(FT_PATH);
        var swPunchReader = new StreamReader(SW_PATH);
        var retractReader = new StreamReader(RETRACT_PATH);
        var ratesReader = new StreamReader(RATES_PATH);

        string featureJson = featurePriceReader.ReadToEnd();
        string tubePunchJson = tubePunchReader.ReadToEnd();
        string soPunchJson = soPunchReader.ReadToEnd();
        string hdsoPunchJson = hdsoPunchReader.ReadToEnd();
        string ftPunchJson = ftPunchReader.ReadToEnd();
        string swPunchJson = swPunchReader.ReadToEnd();
        string retractJson = retractReader.ReadToEnd();
        string ratesJson = ratesReader.ReadToEnd();

        _featurePriceList = JsonConvert.DeserializeObject<List<FeaturePrice>>(featureJson);
        _tubePunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(tubePunchJson);
        _soPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(soPunchJson);
        _hdsoPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(hdsoPunchJson);
        _ftPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(ftPunchJson);
        _swPunchList = JsonConvert.DeserializeObject<List<PunchPrice>>(swPunchJson);
        _retractList = JsonConvert.DeserializeObject<List<PunchPrice>>(retractJson);
        _ratesPrices = JsonConvert.DeserializeObject<RatesPrices>(ratesJson);

        featurePriceReader.Close();
        tubePunchReader.Close();
        soPunchReader.Close();
        hdsoPunchReader.Close();
        ftPunchReader.Close();
        swPunchReader.Close();
        retractReader.Close();
        ratesReader.Close();
    }

    public List<FeaturePrice> GetFeaturePrices() { return _featurePriceList; }
    public RatesPrices GetRates() { return _ratesPrices; }
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

    public async Task<bool> UpdatePunchPrice(PossibleFeatureTypes type, List<PunchPrice> prices)
    {
        bool success;
        switch (type)
        {
            case PossibleFeatureTypes.StdTubePunch:
                _tubePunchList = prices;
                success = await WriteToDBFile("TubePunchPrices", _tubePunchList);
                break;
            case PossibleFeatureTypes.SideOutlet:
                _soPunchList = prices;
                success = await WriteToDBFile("SoPunchPrices", _soPunchList);
                break;
            case PossibleFeatureTypes.HDSideOutlet:
                _hdsoPunchList = prices;
                success = await WriteToDBFile("HdsoPunchPrices", _hdsoPunchList);
                break;
            case PossibleFeatureTypes.StdFTPunch:
                _ftPunchList = prices;
                success = await WriteToDBFile("FtPunchPrices", _ftPunchList);
                break;
            case PossibleFeatureTypes.StdSWPunch:
                _swPunchList = prices;
                success = await WriteToDBFile("SwPunchPrices", _swPunchList);
                break;
            case PossibleFeatureTypes.StdRetractPins:
                _retractList = prices;
                success = await WriteToDBFile("RetractPrices", _retractList);
                break;
            default:
                success = false;
                break;
        }
        return success;
    }

    public async Task<bool> UpdateFeaturePrice(List<FeaturePrice> prices)
    {
        _featurePriceList = prices;
        bool success = await WriteToDBFile("FeaturePrices", _featurePriceList);
        return success;
    }

    public async Task<bool> AddPunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice)
    {
        bool success;
        switch (type)
        {
            case PossibleFeatureTypes.StdTubePunch:
                _tubePunchList.Add(punchPrice);
                success = await WriteToDBFile("TubePunchPrices", _tubePunchList);
                break;
            case PossibleFeatureTypes.SideOutlet:
                _soPunchList.Add(punchPrice);
                success = await WriteToDBFile("SoPunchPrices", _soPunchList);
                break;
            case PossibleFeatureTypes.HDSideOutlet:
                _hdsoPunchList.Add(punchPrice);
                success = await WriteToDBFile("HdsoPunchPrices", _hdsoPunchList);
                break;
            case PossibleFeatureTypes.StdFTPunch:
                _ftPunchList.Add(punchPrice);
                success = await WriteToDBFile("FtPunchPrices", _ftPunchList);
                break;
            case PossibleFeatureTypes.StdSWPunch:
                _swPunchList.Add(punchPrice);
                success = await WriteToDBFile("SwPunchPrices", _swPunchList);
                break;
            case PossibleFeatureTypes.StdRetractPins:
                _retractList.Add(punchPrice);
                success = await WriteToDBFile("RetractPrices", _retractList);
                break;
            default:
                success = false;
                break;
        }
        return success;
    }

    public async Task<bool> UpdateRates(RatesPrices newRates)
    {
        _ratesPrices = newRates;
        return await WriteToDBFile("BasePrices", _ratesPrices);
    }

    /// <summary>
    /// Funcion locks out a file and saves the object to a specified file. It also creates or writes the current
    /// contents to a backup file incase there is an error made while saving.
    /// </summary>
    /// <param name="fileName"> Filename without the extension. </param>
    /// <param name="obj"> The object to write to file. </param>
    /// <returns> Whether the write operation was successful. </returns>
    private async Task<bool> WriteToDBFile(string fileName, Object obj)
    {
        object writeLock = new();
        try
        {
            lock (writeLock)
            {
                // Get current data and save it.
                string path = Path.Combine(BASE_PATH, fileName + ".json");
                var sr = new StreamReader(path);
                string jsonCopy = sr.ReadToEnd();
                sr.Close();

                // Backup current data.
                string backupPath = Path.Combine(BASE_PATH, fileName + "backup.json");
                var backupWriter = new StreamWriter(backupPath);
                Task backupTask = backupWriter.WriteLineAsync(jsonCopy);

                // Finally write new data to file.
                var fileWriter = new StreamWriter(path);
                string toWrite = JsonConvert.SerializeObject(obj);
                Task writeTask = fileWriter.WriteLineAsync(toWrite);

                backupTask.Wait();
                writeTask.Wait();

                fileWriter.Close();
                backupWriter.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return false;
        }
        return true;
    }
}