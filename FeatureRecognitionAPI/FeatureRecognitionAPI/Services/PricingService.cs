using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Services
{
    public class PricingService : IPricingService
    {
        public PricingService() { }

        public async Task<(OperationStatus, string, string?)> EstimatePrice(QuoteSubmissionDto param)
        {
            try
            {
                return (OperationStatus.OK, "Successfully estimated price", "1354.36");
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, ex.Message, null);
            }
        }
    }
}
