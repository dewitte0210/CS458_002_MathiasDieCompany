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
                double totalEstimate = 0.00;
                double totalPerimeter = 0;

                foreach (var feature in param.Features)
                {
                    double setupCost = 0;
                    double oneUpCost = 0;
                    double featureCost = 0;
                    bool isOverSized = feature.Perimeter > 20 ? true : false;
                    int quantity = feature.Count;
                    if (quantity > 500)
                    {
                        //TODO: Need to add some sort of flag to indicate that the quantity is too high
                    }
                    double quantityFactor = GetQuantityFactor(quantity);
                    

                    totalPerimeter += (feature.Perimeter * quantity);

                    if (feature.KissCut)
                    {
                        setupCost = setupCost * 1.15;
                        oneUpCost = oneUpCost * 1.15;
                    }

                    if (isOverSized)
                    {

                    }

                    featureCost = setupCost + (oneUpCost * quantity);
                    totalEstimate += featureCost;
                }

                return (OperationStatus.OK, "Successfully estimated price", totalEstimate.ToString("c"));
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, ex.Message, null);
            }
        }

        private double GetQuantityFactor(int quantity)
        {
            if (quantity >= 1 && quantity <= 12)
                return 1;
            else if (quantity >= 13 && quantity <= 24)
                return .93;
            else if (quantity >= 25 && quantity <= 49)
                return .85;
            else if (quantity >= 50 && quantity <= 99)
                return .79;
            else if (quantity >= 100 && quantity <= 199)
                return .71;
            else if (quantity >= 200 && quantity <= 499)
                return .64;
            else if (quantity >= 500)
                return .63;


        }
    }
}
