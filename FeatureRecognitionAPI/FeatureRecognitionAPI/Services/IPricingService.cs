using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Services
{
    public interface IPricingService
    {
        (OperationStatus, string, string?) EstimatePrice(QuoteSubmissionDto param);
    }
}
