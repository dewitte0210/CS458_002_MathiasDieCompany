using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Services;

public interface IPricingDataService
{
    PunchPriceReturn GetPunchPrices();
    List<FeaturePrice> GetFeaturePrices();
    Task<bool> UpdatePunchPrice(PossibleFeatureTypes type, List<PunchPrice> prices);
    Task<bool> UpdateFeaturePrice(List<FeaturePrice> prices);
    Task<bool> AddPunchPrice(PossibleFeatureTypes type, PunchPrice punchPrice);
}