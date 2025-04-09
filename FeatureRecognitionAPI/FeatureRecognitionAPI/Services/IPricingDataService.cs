using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Services;

public interface IPricingDataService
{
    PunchPriceReturn GetPunchPrices();
    List<FeaturePrice> GetFeaturePrices();
    bool UpdatePunchPrice(PossibleFeatureTypes type, List<PunchPrice> prices);
    bool UpdateFeaturePrice(PossibleFeatureTypes type, List<FeaturePrice> prices);
    bool deletePunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice);
    bool AddPunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice);
}