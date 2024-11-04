using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Services
{
    public interface IPricingService
    {
        Task<(OperationStatus, string, string?)> EstimatePrice(string param);
    }
}
