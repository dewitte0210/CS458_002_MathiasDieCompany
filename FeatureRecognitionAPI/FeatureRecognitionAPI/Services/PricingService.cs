using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

namespace FeatureRecognitionAPI.Services
{
    public class PricingService : IPricingService
    {
        // Change these as necessary
        private const double BASE_SHOP_RATE = 139.10;
        private const double DIE_CUTTING_SHOP_RATE = 126.45;
        private const double PLUG_RATE = 95.17;
        private const double BASE = 130;
        private const double DISCOUNT = 1;

        public PricingService() { }

        public async Task<(OperationStatus, string, string?)> EstimatePrice(QuoteSubmissionDto param)
        {
            try
            {
                if (param.FeatureGroups.Count < 1)
                    return (OperationStatus.BadRequest, "No features detected", null);

                double totalEstimate = 0.00;
                double totalPerimeter = 0.00;
                double ruleFactor = 0.00;
                double setupCostTotal = 0.00;
                double totalFeatureCost = 0.00;

                //Get Punch Lists
                var (tubePunchList, soPunchList, hdsoPunchList, ftPunchList, swPunchList, retractList) = await GetPunchLists();

                //Set rule factor based on rule type
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

                foreach (var feature in param.FeatureGroups.First().Features)
                {
                    double setupCost = 0.00;
                    double runCost = 0.00;
                    double featureCost = 0.00;
                    double featureSetup = 0.00;
                    int maxRadius = 0; // 1 for no max radius
                    bool isOverSized = feature.Perimeter > 20 ? true : false;
                    int quantity = feature.Count * param.FeatureGroups.First().NumberUp;
                    double tempCost = 0.00;

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
                            //Get closest diameter from list
                            var tubePunch = tubePunchList.OrderBy(x => (x.Item1 - feature.Perimeter)).First();
                            setupCost = tubePunch.Item2 * 1.2;
                            runCost = tubePunch.Item3;

                            var punchCost = quantity * runCost;
                            featureCost = punchCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                            continue;
                        case PossibleFeatureTypes.SideOutlet:
                            //Get closest diameter from list
                            var soPunch = soPunchList.OrderBy(x => (x.Item1 - feature.Perimeter)).First();
                            setupCost = soPunch.Item2 * 1.2;
                            runCost = soPunch.Item3;

                            var soCost = quantity * runCost;
                            featureCost = soCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                            continue;
                        case PossibleFeatureTypes.HDSideOutlet:
                            //Get closest diameter from list
                            var hdsoPunch = hdsoPunchList.OrderBy(x => (x.Item1 - feature.Perimeter)).First();
                            setupCost = hdsoPunch.Item2 * 1.2;
                            runCost = hdsoPunch.Item3;

                            var hdsoCost = quantity * runCost;
                            featureCost = hdsoCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                            continue;
                        case PossibleFeatureTypes.StdFTPunch:
                            //Get closest diameter from list
                            var ftPunch = ftPunchList.OrderBy(x => (x.Item1 - feature.Perimeter)).First();
                            setupCost = ftPunch.Item2 * 1.2;
                            runCost = ftPunch.Item3;

                            var ftCost = quantity * runCost;
                            featureCost = ftCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                            continue;
                        case PossibleFeatureTypes.StdSWPunch:
                            //Get closest diameter from list
                            var swPunch = swPunchList.OrderBy(x => (x.Item1 - feature.Perimeter)).First();
                            setupCost = swPunch.Item2 * 1.2;
                            runCost = swPunch.Item3;

                            var swCost = quantity * runCost;
                            featureCost = swCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                            continue;
                        case PossibleFeatureTypes.StdRetractPins:
                            //Get closest diameter from list
                            var retract = retractList.OrderBy(x => (x.Item1 - feature.Perimeter)).First();
                            setupCost = retract.Item2 * 1.2;
                            runCost = retract.Item3;

                            var retractCost = quantity * runCost;
                            featureCost = retractCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                            continue;
                        case PossibleFeatureTypes.Punch:
                            continue;
                        default:
                            break;
                    }

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
                        runCost += (tempCost / 2);
                    }

                    double costSub1 = runCost;
                    double minCost = runCost * 0.25;
                    double costSub2 = 0.00;
                    for (int i = 0; i < quantity; i++)
                    {
                        if (costSub1 > minCost)
                        {
                            featureCost += costSub1;

                            var efficiencySlope = (Math.Sqrt(16 - Math.Pow(0.052915 * i, 2)) - 3.02);

                            costSub2 = costSub1 * efficiencySlope;
                            costSub1 = costSub2;
                        }
                        else
                        {
                            featureCost += minCost;
                        }

                        i++;
                    }
                    
                    featureCost = featureCost * ruleFactor;
                    featureSetup = featureCost * SetupDiscount(feature.Count);
                    totalFeatureCost += featureCost;
                    setupCostTotal += featureSetup;

                    totalPerimeter += (feature.Perimeter * feature.Count);
                }

                double perimeterCost = totalPerimeter * 0.46;    
                totalEstimate = (BASE + setupCostTotal + (DISCOUNT * totalFeatureCost));

                //TODO: implement Ejection method logic (CallPlugTime())

                return (OperationStatus.OK, "Successfully estimated price", totalEstimate.ToString("c"));
            }
            catch (Exception ex)
            {
                return (OperationStatus.ExternalApiFailure, ex.Message, null);
            }
        }

        private double SetupDiscount(int count)
        {
            return count switch
            {
                >= 7 => 0.25,
                >= 6 => 0.46,
                >= 5 => 0.63,
                >= 4 => 0.76,
                >= 3 => 0.86,
                >= 2 => 0.92,
                >= 1 => 1,
                _ => 1,
            };
        }

        private async Task<(List<(double, double, double)>, List<(double, double, double)>, List<(double, double, double)>, List<(double, double, double)>, List<(double, double, double)>, List<(double, double, double)>)> GetPunchLists()
        {
            // Only took unique cut size with highest costs,
            // which is smaller base size from original software list
            var tubePunchList = new List<(double, double, double)>()
            {
                (0.031, 6.45, 4),   // 1/32 Cut
                (0.047, 6.45, 4),   // 3/64 Cut
                (0.062, 5.5, 4),    // 1/16 Cut
                (0.641, 3.5, 5),    // 41/64 Cut
                (0.656, 3.5, 5),    // 21/32 Cut
                (0.656, 3.5, 5),    // 21/32 Cut
                (0.672, 3.5, 5),    // 43/64 Cut
                (0.687, 3.5, 5),    // 11/16 Cut
                (0.703, 3.5, 5),    // 45/64 Cut
                (0.703, 3.5, 5),    // 45/64 Cut
                (0.719, 3.5, 5),    // 23/32 Cut
                (0.719, 3.5, 5),    // 23/32 Cut
                (0.734, 3.5, 5),    // 47/64 Cut
                (0.750, 3.5, 5),    // 3/4 Cut
                (0.766, 3.5, 5),    // 49/64 Cut
                (0.781, 3.5, 5),    // 25/32 Cut
                (0.781, 3.5, 5),    // 25/32 Cut
                (0.797, 3.5, 5),    // 51/64 Cut
                (0.812, 3.5, 5),    // 13/16 Cut
                (0.828, 3.5, 5),    // 53/64 Cut
                (0.828, 3.5, 5),    // 53/64 Cut
                (0.844, 3.5, 5),    // 27/32 Cut
                (0.844, 3.5, 5),    // 27/32 Cut
                (0.859, 3.5, 5),    // 55/64 Cut
                (0.875, 4, 6),      // 7/8 Cut
                (0.891, 4, 6),      // 57/64 Cut
                (0.891, 4, 6),      // 57/64 Cut
                (0.906, 4, 6),      // 29/32 Cut
                (0.906, 4, 6),      // 29/32 Cut
                (0.922, 4, 6),      // 59/64 Cut
                (0.938, 4, 6),      // 15/16 Cut
                (0.953, 4, 6),      // 61/64 Cut
                (0.953, 4, 6),      // 61/64 Cut
                (0.969, 4, 6),      // 31/32 Cut
                (0.984, 4, 6),      // 63/64 Cut
                (1.000, 4, 6),      // 1 Cut
                (1.016, 3.1, 35),   // 1-1/64 Cut
                (1.016, 3.1, 35),   // 1-1/64 Cut
                (1.031, 3.1, 35),   // 1-1/32 Cut
                (1.031, 3.1, 35),   // 1-1/32 Cut
                (1.047, 3.1, 35),   // 1-3/64 Cut
                (1.062, 3.1, 35),   // 1-1/16 Cut
                (1.078, 3.1, 35),   // 1-5/64 Cut
                (1.094, 3.1, 35),   // 1-3/32 Cut
                (1.094, 3.25, 35),  // 1-3/32 Cut
                (1.109, 3.1, 35),   // 1-7/64 Cut
                (1.125, 3.25, 35),  // 1-1/8 Cut
                (1.141, 3.25, 35),  // 1-9/64 Cut
                (1.141, 3.25, 35),  // 1-9/64 Cut
                (1.156, 3.25, 35),  // 1-5/32 Cut
                (1.156, 3.25, 35),  // 1-5/32 Cut
                (1.172, 3.25, 35),  // 1-11/64 Cut
                (1.188, 3.25, 35),  // 1-3/16 Cut
                (1.203, 3.25, 35),  // 1-13/64 Cut
                (1.219, 3.25, 35),  // 1-7/32 Cut
                (1.234, 3.25, 35),  // 1-15/64 Cut
                (1.250, 3.3, 35),   // 1-1/4 Cut
                (1.266, 3.3, 35),   // 1-17/64 Cut
                (1.281, 3.3, 35),   // 1-9/32 Cut
                (1.281, 3.3, 35),   // 1-9/32 Cut
                (1.297, 3.3, 35),   // 1-19/64 Cut
                (1.313, 3.3, 35),   // 1-5/16 Cut
                (1.328, 3.9, 35),   // 1-21/64 Cut
                (1.328, 3.3, 35),   // 1-21/64 Cut
                (1.344, 3.3, 35),   // 1-11/32 Cut
                (1.359, 3.3, 35),   // 1-23/64 Cut
                (1.375, 3.9, 35),   // 1-3/8 Cut
                (1.391, 3.9, 35),   // 1-25/64 Cut
                (1.406, 3.9, 35),   // 1-13/32 Cut
                (1.422, 3.9, 35),   // 1-27/64 Cut
                (1.438, 4.1, 35),   // 1-7/16 Cut
                (1.453, 4.1, 35),   // 1-29/64 Cut
                (1.453, 4.75, 35),  // 1-29/64 Cut
                (1.469, 4.1, 35),   // 1-31/64 Cut
                (1.500, 4.75, 35),  // 1-1/2 Cut
                (1.516, 4.75, 35),  // 1-33/64 Cut
                (1.516, 5.1, 35),   // 1-33/64 Cut
                (1.531, 4.75, 35),  // 1-17/32 Cut
                (1.563, 5.1, 35),   // 1-9/16 Cut
                (1.578, 5.1, 35),   // 1-37/64 Cut
                (1.594, 5.1, 35),   // 1-19/32 Cut
                (1.609, 5.1, 35),   // 1-39/64 Cut
                (1.625, 11.9, 35),  // 1-5/8 Cut
                (1.641, 12.3, 35),  // 1-41/64 Cut
                (1.688, 12.3, 35),  // 1-11/16 Cut
                (1.719, 12.3, 35)   // 1-23/32 Cut
            };

            var soPunchList = new List<(double, double, double)>()
            {
                // (Cut size as a decimal, SetupCost, RunCost)

                (0.031, 6.45, 4),   // 1/32 Cut
                (0.047, 6.45, 4),   // 3/64 Cut
                (0.062, 5.5, 4),    // 1/16 Cut
                (0.078, 11.75, 4),  // 5/64 Cut
                (0.094, 3.8, 4),    // 3/32 Cut
                (0.109, 3.8, 7),    // 7/64 Cut
                (0.125, 3.9, 7),    // 1/8 Cut
                (0.141, 5.05, 7),   // 9/64 Cut
                (0.156, 5.75, 7),   // 5/32 Cut
                (0.172, 4, 7),      // 11/64 Cut
                (0.188, 5.75, 7),   // 3/16 Cut
                (0.203, 5.95, 7),   // 13/64 Cut
                (0.219, 7.55, 7),   // 7/32 Cut
                (0.234, 4.55, 7),   // 15/64 Cut
                (0.25, 8, 7),       // 1/4 Cut
                (0.266, 8.25, 7),   // 17/64 Cut
                (0.281, 7.55, 7),   // 9/32 Cut
                (0.297, 8, 7),      // 19/64 Cut
                (0.312, 7.55, 7),   // 5/16 Cut
                (0.328, 8, 7),      // 21/64 Cut
                (0.344, 14.05, 7),  // 11/32 Cut
                (0.359, 8, 7),      // 23/64 Cut
                (0.375, 12.2, 7),   // 3/8 Cut
                (0.391, 14.05, 7),  // 25/64 Cut
                (0.406, 16, 7),     // 13/32 Cut
                (0.422, 14.05, 7),  // 27/64 Cut
                (0.438, 12.2, 7),   // 7/16 Cut
                (0.453, 14.05, 7),  // 29/64 Cut
                (0.469, 12.2, 7),   // 15/32 Cut
                (0.484, 14.05, 7),  // 31/64 Cut
                (0.5, 12.2, 7),     // 1/2 Cut
                (0.531, 14.05, 7),  // 17/32 Cut
                (0.562, 14.05, 7),  // 9/16 Cut
                (0.578, 14.05, 7),  // 37/64 Cut
                (0.609, 14.05, 7),  // 39/64 Cut
                (0.625, 16, 7)      // 5/8 Cut
            };

            var hdsoPunchList = new List<(double, double, double)>()
            {
                (0.062, 6.5, 9),   // 1/16 Cut
                (0.109, 6.5, 9),   // 7/64 Cut
                (0.125, 6.5, 9),   // 1/8 Cut
                (0.141, 6.5, 9),   // 9/64 Cut
                (0.156, 6.5, 9),   // 5/32 Cut
                (0.172, 6.5, 9),   // 11/64 Cut
                (0.188, 6.5, 9),   // 3/16 Cut
                (0.203, 6.5, 9),   // 13/64 Cut
                (0.219, 6.5, 9),   // 7/32 Cut
                (0.234, 6.5, 9),   // 15/64 Cut
                (0.25, 8, 9),      // 1/4 Cut
                (0.312, 8, 9),     // 5/16 Cut
                (0.375, 8, 9),     // 3/8 Cut
                (0.5, 8, 9)        // 1/2 Cut
            };

            var ftPunchList = new List<(double, double, double)>()
            {
                (0.031, 8.5, 3),   // 1/32 Cut, 1/8 Base
                (0.031, 8.5, 3),   // 1/32 Cut, 3/16 Base
                (0.047, 8.5, 3),   // 3/64 Cut, 1/8 Base
                (0.062, 8.5, 3),   // 1/16 Cut, 1/8 Base
                (0.062, 8.5, 3),   // 1/16 Cut, 3/16 Base
                (0.078, 8.5, 3),   // 5/64 Cut, 3/16 Base
                (0.093, 4, 3),     // 3/32 Cut, 3/16 Base
                (0.109, 4, 3),     // 7/64 Cut, 3/16 Base
                (0.125, 4, 3),     // 1/8 Cut, 3/16 Base
                (0.125, 4, 3),     // 1/8 Cut, 1/4 Base
                (0.140, 4, 3),     // 9/64 Cut, 1/4 Base
                (0.156, 4, 3),     // 5/32 Cut, 1/4 Base
                (0.171, 4, 3),     // 11/64 Cut, 1/4 Base
                (0.187, 4, 3),     // 3/16 Cut, 1/4 Base
                (0.187, 4, 3),     // 3/16 Cut, 5/16 Base
                (0.203, 4, 3),     // 13/64 Cut, 5/16 Base
                (0.218, 4, 3),     // 7/32 Cut, 5/16 Base
                (0.234, 4, 3),     // 15/64 Cut, 5/16 Base
                (0.250, 4, 3),     // 1/4 Cut, 5/16 Base
                (0.250, 4, 3),     // 1/4 Cut, 3/8 Base
                (0.265, 4, 3),     // 17/64 Cut, 3/8 Base
                (0.281, 4, 3),     // 9/32 Cut, 3/8 Base
                (0.296, 4, 3),     // 19/64 Cut, 3/8 Base
                (0.312, 4, 3),     // 5/16 Cut, 3/8 Base
                (0.312, 4, 3),     // 5/16 Cut, 7/16 Base
                (0.328, 4, 3),     // 21/64 Cut, 7/16 Base
                (0.343, 4, 3),     // 11/32 Cut, 7/16 Base
                (0.359, 4, 3),     // 23/64 Cut, 7/16 Base
                (0.375, 4, 3),     // 3/8 Cut, 7/16 Base
                (0.375, 4, 3),     // 3/8 Cut, 1/2 Base
                (0.390, 4, 3),     // 25/64 Cut, 1/2 Base
                (0.406, 4, 3),     // 13/32 Cut, 1/2 Base
                (0.421, 4, 3),     // 27/64 Cut, 1/2 Base
                (0.437, 4, 3),     // 7/16 Cut, 1/2 Base
                (0.437, 4, 3),     // 7/16 Cut, 9/16 Base
                (0.453, 4, 3),     // 29/64 Cut, 9/16 Base
                (0.468, 4, 3),     // 15/32 Cut, 9/16 Base
                (0.484, 4, 3),     // 31/64 Cut, 9/16 Base
                (0.500, 4, 3),     // 1/2 Cut, 9/16 Base
                (0.500, 4, 3),     // 1/2 Cut, 5/8 Base
                (0.515, 4, 3),     // 33/64 Cut, 5/8 Base
                (0.531, 4, 3),     // 17/32 Cut, 5/8 Base
                (0.546, 4, 3),     // 35/64 Cut, 5/8 Base
                (0.562, 4, 3),     // 9/16 Cut, 11/16 Base
                (0.578, 4, 3),     // 37/64 Cut, 11/16 Base
                (0.593, 4, 3),     // 19/32 Cut, 11/16 Base
                (0.609, 4, 3),     // 39/64 Cut, 11/16 Base
                (0.625, 4, 3),     // 5/8 Cut, 3/4 Base
                (0.640, 4, 3),     // 41/64 Cut, 3/4 Base
                (0.656, 4, 3),     // 21/32 Cut, 3/4 Base
                (0.687, 4, 3),     // 11/16 Cut, 13/16 Base
                (0.703, 4, 3),     // 45/64 Cut, 13/16 Base
                (0.718, 4, 3),     // 23/32 Cut, 13/16 Base
                (0.750, 4, 3),     // 3/4 Cut, 7/8 Base
                (0.765, 4, 3),     // 49/64 Cut, 7/8 Base
                (0.781, 4, 3),     // 25/32 Cut, 7/8 Base
                (0.812, 6, 3),     // 13/16 Cut, 15/16 Base
                (0.828, 6, 3),     // 53/64 Cut, 15/16 Base
                (0.843, 6, 3),     // 27/32 Cut, 15/16 Base
                (0.875, 6, 3),     // 7/8 Cut, 1 Base
                (0.890, 6, 3),     // 57/64 Cut, 1 Base
                (0.906, 6, 3),     // 29/32 Cut, 1 Base
                (0.937, 6, 3),     // 15/16 Cut, 1-1/16 Base
                (1.000, 6, 3),     // 1 Cut, 1-1/8 Base
                (1.016, 15, 3),    // 1-1/64 Cut, 1-1/8 Base
                (1.031, 15, 3),    // 1-1/32 Cut, 1-1/8 Base
                (1.125, 15, 3),    // 1-1/8 Cut, 1-1/4 Base
                (1.188, 15, 3),    // 1-3/16 Cut, 1-5/16 Base
                (1.375, 15, 3)     // 1-3/8 Cut, 1-1/2 Base
            };

            var swPunchList = new List<(double, double, double)>()
            {
                (0.078, 7.8, 3),   // 5/64 Cut
                (0.094, 7.1, 3),   // 3/32 Cut
                (0.109, 6.45, 3),  // 7/64 Cut
                (0.125, 6.45, 3),  // 1/8 Cut
                (0.141, 6, 3),     // 9/64 Cut
                (0.156, 6.35, 3),  // 5/32 Cut
                (0.172, 5, 3),     // 11/64 Cut
                (0.188, 6, 3),     // 3/16 Cut
                (0.203, 5.95, 3),  // 13/64 Cut
                (0.219, 6.05, 3),  // 7/32 Cut
                (0.234, 3, 3),     // 15/64 Cut
                (0.250, 6, 3),     // 1/4 Cut
                (0.266, 6.1, 3),   // 17/64 Cut
                (0.281, 6.2, 3),   // 9/32 Cut
                (0.297, 3.25, 4),  // 19/64 Cut
                (0.313, 6.2, 3),   // 5/16 Cut
                (0.328, 6, 3),     // 21/64 Cut
                (0.344, 6.1, 3),   // 11/32 Cut
                (0.359, 3.25, 4),  // 23/64 Cut
                (0.375, 6.5, 3),   // 3/8 Cut
                (0.391, 6.2, 3),   // 25/64 Cut
                (0.406, 6.5, 3),   // 13/32 Cut
                (0.422, 3.25, 4),  // 27/64 Cut
                (0.438, 6.7, 3),   // 7/16 Cut
                (0.453, 6.35, 3),  // 29/64 Cut
                (0.469, 6.7, 3),   // 15/32 Cut
                (0.484, 3.25, 4),  // 31/64 Cut
                (0.500, 6.85, 3),  // 1/2 Cut
                (0.516, 8, 3)      // 33/64 Cut
            };

            var retractList = new List<(double, double, double)>()
            {
                (0.125, 8, 4), // 1/8th Square/Dome top
                (.25, 11, 4) // 1/4 Square top
            };

            return (tubePunchList, soPunchList, hdsoPunchList, ftPunchList, swPunchList, retractList);
        }
    }
}
