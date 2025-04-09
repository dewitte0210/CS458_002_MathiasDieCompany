using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Services
{
    public interface IPricingService
    {
        (OperationStatus, string, string?) EstimatePrice(QuoteSubmissionDto param);
    }
}
