using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Services
{
    public class PricingService : IPricingService
    {
        private const double BASE_SHOP_RATE = 139.10;
        private const double DIE_CUTTING_SHOP_RATE = 126.45;
        private const double PLUG_RATE = 95.17;

        public PricingService() {}

        public async Task<(OperationStatus, string, string?)> EstimatePrice(QuoteSubmissionDto param)
        {
            try
            {
                double runningCost = 0;

                //Calculate total perimeter cost

                foreach (var feature in param.Features)
                {

                }
                return (OperationStatus.OK, "Successfully estimated price", "1354.36");
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, ex.Message, null);
            }
        }
    }
}
