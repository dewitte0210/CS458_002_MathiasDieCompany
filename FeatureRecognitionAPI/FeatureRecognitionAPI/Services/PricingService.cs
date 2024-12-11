using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace FeatureRecognitionAPI.Services
{
    public class PricingService : IPricingService
    {
        private const double BASE_SHOP_RATE = 139.10;
        private const double DIE_CUTTING_SHOP_RATE = 126.45;
        private const double PLUG_RATE = 95.17;

        public PricingService() { }

        public async Task<(OperationStatus, string, string?)> EstimatePrice(QuoteSubmissionDto param)
        {
            try
            {
                if (param.FeatureList.Count < 1)
                    return (OperationStatus.BadRequest, "No features detected", null);

                double totalEstimate = 0.00;
                double totalPerimeter = 0.00;
                double ruleFactor = 0.00;

                //Get Punch Lists
                var (tubePunchList, soPunchList, hdsoPunchList, ftPunchList, swPunchList, retractList) = await GetPunchLists();

                switch (param.RuleType)
                {
                    case RuleTypeEnum.TwoPtCB937:
                        ruleFactor = 1; break;
                    case RuleTypeEnum.TwoPtSB937:
                        ruleFactor = 1.3; break;
                    case RuleTypeEnum.TwoPtDDB937:
                        ruleFactor = 1.1; break;
                    case RuleTypeEnum.TwoPtCB1125:
                        ruleFactor = 1.25; break;
                    case RuleTypeEnum.ThreePtCB937:
                        ruleFactor = 1.3; break;
                    case RuleTypeEnum.ThreePtSB937:
                        ruleFactor = 1.5; break;
                    case RuleTypeEnum.ThreePtDDB937:
                        ruleFactor = 1.3; break;
                    case RuleTypeEnum.ThreePtDSB927:
                        ruleFactor = 1.5; break;
                    case RuleTypeEnum.FourTwelveCB472:
                        ruleFactor = 1.3; break;
                    case RuleTypeEnum.FiveTwelveCB472:
                        ruleFactor = 1.3; break;
                }

                foreach (var tempfeature in param.FeatureList)
                {
                    #region Feature Type Cost Calculation
                    double setupCost = 0.00;
                    double runCost = 0.00;
                    double featureCost = 0.00;
                    var feature = tempfeature.Features.First();
                    int maxRadius = 0; // 1 for no max radius

                    // Setup Cost = hour/part to setup * ShopRate $/hour
                    // Run cost is calculated using the following factors and variables
                    // Difficulty Factor * mMain.ShopRate $/hr * hour/part * quantity of parts
                    switch (feature.FeatureType)
                    {
                        case PossibleFeatureTypes.Group1A1: // Metered Rectangle
                            setupCost = 0.22 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.04 * 4;
                            maxRadius = 4;
                            break;
                        case PossibleFeatureTypes.Group1A2: // Radius Corner Rectangle
                            setupCost = 0.22 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.04 * 4;
                            maxRadius = 4;
                            break;
                        case PossibleFeatureTypes.Group1B1: // Circle
                            setupCost = 0.3 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.042 * 4;
                            maxRadius = 1;
                            break;
                        case PossibleFeatureTypes.Group1B2: // Rounded Rectangle
                            setupCost = 0.3 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.042 * 4;
                            maxRadius = 1;
                            break;
                        case PossibleFeatureTypes.Group1C: // Triangles
                            setupCost = 0.32 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.045 * 6;
                            maxRadius = 3;
                            break;
                        case PossibleFeatureTypes.Group2A1: // Elipses
                            setupCost = 0.9 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.045 * 6;
                            maxRadius = 1;
                            break;
                        case PossibleFeatureTypes.Group2A2: // Bowties
                            setupCost = 0.9 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.045 * 6;
                            maxRadius = 1;
                            break;
                        case PossibleFeatureTypes.Group3: // Corner Chamfer
                            setupCost = 0.14 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.04 * 2;
                            maxRadius = 2;
                            break;
                        case PossibleFeatureTypes.Group4: // V-Notch & Corner Notch
                            setupCost = 0.25 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.04 * 3;
                            maxRadius = 3;
                            break;
                        case PossibleFeatureTypes.Group5: // Mitered Notches
                            setupCost = 0.25 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.04 * 4;
                            maxRadius = 2;
                            break;
                        case PossibleFeatureTypes.Group6: // Radius Notches
                            setupCost = 0.33 * BASE_SHOP_RATE;
                            runCost = 1 * BASE_SHOP_RATE * 0.04 * 4;
                            maxRadius = 4;
                            break;
                        case PossibleFeatureTypes.StdTubePunch:
                            // Cut size will be diameter returned, just choose the higher priced base size for punches
                            break;
                        case PossibleFeatureTypes.SideOutlet:
                            break;
                        case PossibleFeatureTypes.HDSideOutlet:
                            break;
                        case PossibleFeatureTypes.StdFTPunch:
                            break;
                        case PossibleFeatureTypes.StdSWPunch:
                            break;
                        case PossibleFeatureTypes.StdRetractPins:
                            break;
                        case PossibleFeatureTypes.Punch:
                            break;
                        default:
                            break;
                    }
                    #endregion

                    bool isOverSized = feature.Perimeter > 20 ? true : false;
                    int quantity = feature.Count * param.FeatureList.First().NumberUp;
                    double quantityFactor = GetQuantityFactor(quantity);
                    double tempCost = 0.00;

                    if (feature.MultipleRadius)
                    {
                        if (maxRadius == 1)
                        {
                            // TODO: setupCost = setupCost * 0.7 * Quantity of unique radius sizes;
                            runCost = runCost * 1.1;
                        }
                    }

                    if (feature.KissCut)
                    {
                        setupCost = setupCost * 1.15;
                        runCost = runCost * 1.15;
                    }

                    if (isOverSized)
                    {
                        // NOTE: Currently feature detection does not support knifed features
                        tempCost = feature.Perimeter * .19;
                        runCost += tempCost;
                    }

                    int baseCost = 0;
                    double discount = 0.00;
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
            else
                return 1;
        }

        private async Task<(List<(double, double)>, List<(decimal, decimal)>, List<(decimal, decimal)>, List<(decimal, decimal)>, List<(decimal, decimal)>, List<(decimal, decimal)>)> GetPunchLists()
        {
            // Only took unique cut size with highest costs,
            // which is smaller base size from original software list

            var soPunchList = new List<(double, double)>()
            {
                // (SetupCost, RunCost)

                (6.45, 4),   // 1/32 Cut
                (6.45, 4),   // 3/64 Cut
                (5.5, 4),    // 1/16 Cut
                (11.75, 4),  // 5/64 Cut
                (3.8, 4),    // 3/32 Cut
                (3.8, 7),    // 7/64 Cut
                (3.9, 7),    // 9/64 Cut
                (5.05, 7),   // 11/64 Cut
                (5.75, 7),   // 3/16 Cut
                (4, 7),      // 7/32 Cut
                (5.75, 7),   // 17/64 Cut
                (5.95, 7),   // 21/64 Cut
                (7.55, 7),   // 11/32 Cut
                (4.55, 7),   // 23/64 Cut
                (8, 7),      // 3/8 Cut
                (8.25, 7),   // 25/64 Cut
                (7.55, 7),   // 13/32 Cut
                (8, 7),      // 27/64 Cut
                (7.55, 7),   // 7/16 Cut
                (8, 7),      // 29/64 Cut
                (14.05, 7),  // 15/32 Cut
                (8, 7),      // 31/64 Cut
                (12.2, 7),   // 1/2 Cut
                (14.05, 7),  // 37/64 Cut
                (16, 7)      // 5/8 Cut
            };

            var hdsoPunchList = new List<(double, double)>()
            {

            };
        }
    }
}
