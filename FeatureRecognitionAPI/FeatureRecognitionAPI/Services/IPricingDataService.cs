using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Services;

public interface IPricingDataService
{
    PunchPriceReturn GetPunchPrices();
    List<FeaturePrice> GetFeaturePrices();
}