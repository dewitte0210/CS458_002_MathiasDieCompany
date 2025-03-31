using System.Globalization;
using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Services
{
    public class PricingService : IPricingService
    {
        // Change these as necessary
        private const double BASE_SHOP_RATE = 139.10;
        
        // This only appears to be used for some sort of report calc that we are not doing
        private const double DIE_CUTTING_SHOP_RATE = 126.45;
        // Not entirely sure when this comes into play. Looks like it is involved in the final price calc 
        private const double PLUG_RATE = 95.17;
        
        //private const double BASE = 130; This is the main1 number
        private const double BASE = 60; 
        private const double DISCOUNT = 1;
        private readonly List<PunchPrice> _tubePunchList, _soPunchList, _hdsoPunchList,
            _ftPunchList, _swPunchList, _retractList;
        private readonly List<FeaturePrice> _featurePriceList;
        
        public PricingService()
        {
                //Get Feature Price Data
                _featurePriceList = GetFeaturePriceList();
                
                //Get Punch Lists
                _tubePunchList = GetTubePunchList();
                _soPunchList = GetSoPunchList();
                _hdsoPunchList = GetHdsoPunchList();
                _ftPunchList = GetFtPunchList();
                _swPunchList = GetSwPunchList();
                _retractList = GetRetractList();
        }

        public (OperationStatus, string, string?) EstimatePrice(QuoteSubmissionDto param)
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


                //Set rule factor based on rule type
                ruleFactor = param.RuleType switch
                {
                    RuleTypeEnum.TwoPtCB937 => 1,
                    RuleTypeEnum.TwoPtSB937 => 1.3,
                    RuleTypeEnum.TwoPtDDB937 => 1.1,
                    RuleTypeEnum.TwoPtCB1125 => 1.25,
                    RuleTypeEnum.ThreePtCB937 => 1.3,
                    RuleTypeEnum.ThreePtSB937 => 1.5,
                    RuleTypeEnum.ThreePtDDB937 => 1.3,
                    RuleTypeEnum.ThreePtDSB927 => 1.5,
                    RuleTypeEnum.FourTwelveCB472 => 1.3,
                    RuleTypeEnum.FiveTwelveCB472 => 1.3,
                    _ => ruleFactor
                };

                foreach (var feature in param.FeatureGroups.First().Features)
                {
                    double setupCost = 0.00;
                    double runCost = 0.00;
                    double featureCost = 0.00;
                    int maxRadius = 0; // 1 for no max radius
                    bool isOverSized = feature.Perimeter > 20;
                    int quantity = feature.Count * param.FeatureGroups.First().NumberUp;

                    // Setup Cost = hour/part to set up * ShopRate $/hour
                    // Run cost is calculated using the following factors and variables
                    // Difficulty Factor * mMain.ShopRate $/hr * hour/part * quantity of parts
                    // For punches, there should be a checkbox that denotes if the height of the punch is .918 instead
                    // of .937
                    /*
                        Setupcost = 0.22 * mMain.Shoprate
                        RunCost = 1 * mMain.Shoprate * 0.04 * 4
                    */
                    
                    FeaturePrice? featureData =
                        _featurePriceList.Find(element => element.Type == feature.FeatureType);
                    
                    // If the current feature is a Punch featureData will be null, and we must use punch pricing rules
                    if (featureData != null)
                    {
                        setupCost = featureData.SetupRate * BASE_SHOP_RATE;
                        runCost = featureData.DifficultyFactor * BASE_SHOP_RATE * featureData.RunRate * featureData.Quantity;
                       
                        if (feature.MultipleRadius > 1)
                        {
                            if (featureData.MaxRadius != 1)
                            {
                                setupCost = setupCost * 0.7 * feature.MultipleRadius;
                                runCost *= 1.1;
                            }
                        }
                          
                        if (feature.KissCut)
                        {
                            setupCost *= 1.15;
                            runCost *= 1.15;
                        }

                        if (isOverSized)
                        {
                            // NOTE: Currently feature detection does not support knifed features
                            var tempCost = feature.Perimeter * .19;
                            runCost += (tempCost / 2);
                        }

                        // Includes a discount depending on the quantity of the feature
                        double costSub1 = runCost;
                        double minCost = runCost * 0.25;
                        for (int i = 0; i < quantity; i++)
                        {
                            if (costSub1 > minCost)
                            {
                                featureCost += costSub1;

                               // var efficiencySlope = (Math.Sqrt(16 - Math.Pow(0.052915 * i, 2)) - 3.02); this is the main 1 number
                               var efficiencySlope = 0.98; 
                               costSub1 *= efficiencySlope;
                            }
                            else
                            {
                                featureCost += minCost;
                            }

                            i++;
                        }
                    
                        featureCost *= ruleFactor;
                        var featureSetup = setupCost * SetupDiscount(quantity);
                        totalFeatureCost += featureCost;
                        setupCostTotal += featureSetup;

                        totalPerimeter += (feature.Perimeter * quantity);
                    }
                    else
                    { 
                        // For punch pricing we get the closest punch by comparing the perimeter with the punch sizes in our lists 
                        PunchPrice punch; 
                        switch (feature.FeatureType)
                        { 
                            case PossibleFeatureTypes.StdTubePunch:
                                punch = _tubePunchList.OrderBy(x => (Math.Abs(x.CutSize - feature.Perimeter))).First();
                                break;
                            case PossibleFeatureTypes.SideOutlet:
                                punch = _soPunchList.OrderBy(x => (x.CutSize - feature.Perimeter)).First();
                                break;
                            case PossibleFeatureTypes.HDSideOutlet:
                                punch = _hdsoPunchList.OrderBy(x => (x.CutSize - feature.Perimeter)).First();
                                break;
                            case PossibleFeatureTypes.StdFTPunch:
                                punch = _ftPunchList.OrderBy(x => (x.CutSize - feature.Perimeter)).First();
                                break;
                            case PossibleFeatureTypes.StdSWPunch:
                                punch = _swPunchList.OrderBy(x => (x.CutSize - feature.Perimeter)).First();
                                break; 
                            case PossibleFeatureTypes.StdRetractPins:
                                punch = _retractList.OrderBy(x => (x.CutSize - feature.Perimeter)).First();
                                break;
                            // Not sure if Punch is even valid for pricing 
                            case PossibleFeatureTypes.Punch:
                            default:
                                continue;
                        }
                            setupCost = punch.SetupCost * 1.2;
                            runCost = punch.RunCost;

                            var punchCost = quantity * runCost;// * PunchPrice.PunchDiscount(feature.FeatureType, quantity);
                            featureCost = punchCost + (quantity * setupCost);
                            totalFeatureCost += featureCost;
                    }
                  
                }

                double perimeterCost = totalPerimeter * 0.46;    
                totalEstimate = (BASE + setupCostTotal + (DISCOUNT * totalFeatureCost)) + perimeterCost;

                return (OperationStatus.OK, "Successfully estimated price", totalEstimate.ToString(CultureInfo.CurrentCulture));
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
       
        private List<FeaturePrice> GetFeaturePriceList()
        {
            // Features with maxRadius of 1 cannot have multi-radius calculation
            // Groups 15 and 16 dont have any price calculations attached
           var features = new List<FeaturePrice>()
           {
               new FeaturePrice(PossibleFeatureTypes.Group1A1,  0.22, 0.04, 1, 4, 4),
               new FeaturePrice(PossibleFeatureTypes.Group1A2, 0.22, 0.04, 1, 4, 4),
               new FeaturePrice(PossibleFeatureTypes.Group1B1, 0.3, 0.042, 1, 1, 4),
               new FeaturePrice(PossibleFeatureTypes.Group1B2, 0.3, 0.042, 1, 1, 4),
               new FeaturePrice(PossibleFeatureTypes.Group1C, 0.32, 0.045, 1, 3, 6),
               new FeaturePrice(PossibleFeatureTypes.Group2A1, 0.9, 0.045, 1, 1, 6),
               new FeaturePrice(PossibleFeatureTypes.Group2A2, 0.9, 0.045, 1, 1, 6),
               new FeaturePrice(PossibleFeatureTypes.Group3, 0.14, 0.04, 1, 2, 1),
               new FeaturePrice(PossibleFeatureTypes.Group4, 0.25, 0.04, 1, 3, 3),
               new FeaturePrice(PossibleFeatureTypes.Group5, 0.25, 0.04, 1, 2, 4),
               new FeaturePrice(PossibleFeatureTypes.Group6, 0.33, 0.04, 1, 4, 4),
               new FeaturePrice(PossibleFeatureTypes.Group7, 0.5, 0.08, 1, 1, 4), 
               new FeaturePrice(PossibleFeatureTypes.Group8, 0.6, 0.042, 1, 6, 8),
               new FeaturePrice(PossibleFeatureTypes.Group9, 0.25, 0.035, 0.75, 2, 6),
               new FeaturePrice(PossibleFeatureTypes.Group10, 0.63, 0.06, 1, 1, 10),
               new FeaturePrice(PossibleFeatureTypes.Group11, 0.3333, 0.04, 1, 1, 7),
               new FeaturePrice(PossibleFeatureTypes.Group12, 0.3, 0.04, 1, 1, 6),
               new FeaturePrice(PossibleFeatureTypes.Group13, 0.6, 0.035, 1,6, 8),
               new FeaturePrice(PossibleFeatureTypes.Group14, 0.2, 0.023,1,1, 4),
               new FeaturePrice(PossibleFeatureTypes.Group17, 0.58, 0.04, 1, 1, 8),
               new FeaturePrice(PossibleFeatureTypes.GroupS1, 0.19, 0.019, 1, 1, 6),
               new FeaturePrice(PossibleFeatureTypes.GroupS2, 0.2, 0.018, 1, 1, 8),
               new FeaturePrice(PossibleFeatureTypes.GroupS3, 0.21, 0.017, 1, 1, 10),
               new FeaturePrice(PossibleFeatureTypes.GroupS4, 0.22, 0.016, 1, 1, 12),
           };
           return features;
        }
        
        private List<PunchPrice> GetTubePunchList()
        {
            var tubePunchList = new List<PunchPrice>()
            {
                new PunchPrice(3/32.0, 1/8.0, 4.5, 3),
                new PunchPrice(3/32.0, 3/16.0, 4.5, 3),
                new PunchPrice(7/64.0, 1/8.0, 4.5, 3),
                new PunchPrice(7/64.0, 3/16.0, 3, 3),
                new PunchPrice(1/8.0, 3/16.0, 3, 3),
                new PunchPrice(9/64.0, 3/16.0, 3, 3),
                new PunchPrice(9/64.0, 1/4.0, 3, 3),
                new PunchPrice(5/32.0, 3/16.0, 3, 3),
                new PunchPrice(5/32.0, 1/4.0, 3, 3),
                new PunchPrice(11/64.0, 3/16.0, 3, 3),
                new PunchPrice(3/16.0, 1/4.0, 3, 3),
                new PunchPrice(13/64.0, 1/4.0, 3, 3 ),
                new PunchPrice(13/64.0, 5/16.0, 3, 3),
                new PunchPrice(7/32.0, 1/4.0, 3, 3),
                new PunchPrice(7/32.0, 5/16.0, 3,3),
                new PunchPrice(15/64.0, 1/4.0, 3,3),
                new PunchPrice(1/4.0, 5/16.0, 3,3),
                new PunchPrice(17/64.0, 5/16.0, 3, 3),
                new PunchPrice(17/64.0, 3/8.0, 3, 3),            
                new PunchPrice(9/32.0, 5/16.0, 3.25, 4),
                new PunchPrice(19/64.0, 5/16.0, 3.25, 4),
                new PunchPrice(5/16.0, 3/8.0, 3.25, 4),
                new PunchPrice(21/64.0, 3/8.0, 3.25, 4),
                new PunchPrice(21/64.0, 7/16.0, 3.25, 4),
                new PunchPrice(11/32.0, 3/8.0, 3.25, 4),
                new PunchPrice(11/32.0, 7/16.0, 3.25, 4),
                new PunchPrice(23/64.0, 3/8.0, 3.25, 4),
                new PunchPrice(3/8.0, 7/16.0, 3.25, 4), 
                new PunchPrice(25/64.0, 7/16.0, 3.25, 4),
                new PunchPrice(25/64.0, 1/2.0, 3.25, 4),
                new PunchPrice(13/32.0, 7/16.0, 3.25, 4),
                new PunchPrice(13/32.0, 1/2.0, 3.25, 4),
                new PunchPrice(27/64.0, 7/16.0, 3.25, 4),
                new PunchPrice(7/16.0, 1/2.0, 3.25, 4),
                new PunchPrice(29/64.0, 1/2.0, 3.25, 4),
                new PunchPrice(15/32.0, 1/2.0, 3.25, 4),
                new PunchPrice(15/32.0, 9/16.0, 3.25, 4),
                new PunchPrice(31/64.0, 1/2.0, 3.25, 4),
                new PunchPrice(1/2.0, 9/16.0, 3.25, 4),
                new PunchPrice(33/64.0, 9/16.0, 3.25, 4),
                new PunchPrice(33/64.0, 5/8.0, 3.25, 4),
                new PunchPrice(17/32.0, 9/16.0, 3.25, 4),
                new PunchPrice(17/32.0, 5/8.0, 3.25, 4),
                new PunchPrice(35/64.0, 9/16.0, 3.25, 4),
                new PunchPrice(9/16.0, 5/8.0, 3.25,4),
                new PunchPrice(37/64.0, 5/8.0, 3.5, 5),
                new PunchPrice(19/32.0, 5/8.0, 3.5, 5),
                new PunchPrice(19/32.0, 11/16.0, 3.5, 5),
                new PunchPrice(39/64.0, 5/8.0, 3.5, 5),
                new PunchPrice(5/8.0, 11/16.0, 3.5, 5),
                new PunchPrice(41/64.0, 11/16.0, 3.5, 5),
                new PunchPrice(21/32.0, 11/16.0, 3.5, 5),
                new PunchPrice(21/32.0, 3/4.0, 3.5, 5),
                new PunchPrice(43/64.0, 11/16.0, 3.5, 5),
                new PunchPrice(11/16.0, 3/4.0, 3.5, 5),
                new PunchPrice(45/64.0, 3/4.0, 3.5, 5),
                new PunchPrice(45/64.0, 13/16.0, 3.5, 5),           
                new PunchPrice(23/32.0, 3/4.0, 3.5, 5),   
                new PunchPrice(23/32.0, 13/16.0, 3.5, 5),
                new PunchPrice(47/64.0, 3/4.0, 3.5, 5),
                new PunchPrice(3/4.0, 13/16.0, 3.5, 5),
                new PunchPrice(49/64.0, 13/16.0, 3.5, 5),
                new PunchPrice(25/32.0, 13/16.0, 3.5, 5),
                new PunchPrice(25/32.0, 7/8.0, 3.5, 5),
                new PunchPrice(51/64.0, 13/64.0, 3.5,5),
                new PunchPrice(13/16.0, 7/8.0, 3.5,5),
                new PunchPrice(53/64.0, 7/8.0, 3.5, 5),
                new PunchPrice(53/64.0, 15/16.0, 3.5,5),
                new PunchPrice(27/32.0, 7/8.0, 3.5, 5),
                new PunchPrice(27/32.0, 15/16.0, 3.5, 5),
                new PunchPrice(55/64.0, 7/8.0, 3.5,5),
                new PunchPrice(7/8.0, 15/16.0, 4, 6),
                new PunchPrice(57/64.0, 15/16.0, 4, 6),
                new PunchPrice(57/64.0, 1, 4, 6),
                new PunchPrice(29/32.0, 15/16.0, 4, 6),
                new PunchPrice(29/32.0, 1, 4, 6),
                new PunchPrice(59/64.0, 15/16.0, 4,6),
                new PunchPrice(15/16.0, 1, 4, 6),
                new PunchPrice(61/64.0, 1, 4, 6),
                new PunchPrice(61/64.0, (1 + 1/16.0), 4, 6),
                new PunchPrice(31/32.0, 1, 4, 6),
                new PunchPrice(63/64.0, 1, 4, 6),
                new PunchPrice(1, (1 + 1/16.0), 4, 6),
                new PunchPrice((1 + 1/64.0), (1 + 1/16.0), 3.2, 35),
                new PunchPrice( 1 + (1/64.0), 1 + (1/8.0), 3.1, 35),
                new PunchPrice(1 + (1/32.0), 1 + (1/16.0), 3.1, 35),
                new PunchPrice(1 + (1/32.0), 1 + (1/8.0), 3.1, 35),
                new PunchPrice(1 + (3/64.0), 1 + (1/16.0), 3.1, 35),
                new PunchPrice(1 + (1/16.0), 1 + (1/8.0), 3.1, 35),
                new PunchPrice(1 + (5/64.0), 1 + (1/8.0), 3.1, 35),
                new PunchPrice(1 + (3/32.0), 1 + (1/8.0), 3.1, 35),
                new PunchPrice(1 + (3/32.0), 1 + (3/16.0), 3.25, 35),
                new PunchPrice(1 + (7/64.0), 1 + (1/8.0), 3.1, 35),
                new PunchPrice(1 + (1/8.0), 1 + (3/16.0), 3.25, 35),
                new PunchPrice(1 + (9/64.0), 1 + (3/16.0), 3.25, 35),
                new PunchPrice(1 + 9/64.0, 1 + (1/4.0), 3.25, 35),
                new PunchPrice(1 + (5/32.0), 1 + (3/16.0), 3.25, 35) ,
                new PunchPrice(1 + 5/32.0, 1 + (1/4.0), 3.25, 35),
                new PunchPrice(1 + (11/64.0), 1 + (3/16.0), 3.25, 35),
                new PunchPrice(1 + (3/16.0), 1 + (1/4.0), 3.25, 35),
                new PunchPrice(1 + (13/64.0), 1 + (1/4.0), 3.25, 35),
                new PunchPrice(1 + (7/32.0), 1 + (1/4.0), 3.25, 35),
                new PunchPrice(1 + (15/64.0), 1 + (1/4.0), 3.25, 35),
                new PunchPrice(1 + (1/4.0), 1 + (5/16.0), 3.3, 35),
                new PunchPrice(1 + (17/64.0), 1 + (5/16.0), 3.3, 35),
                new PunchPrice(1 + (9/32.0), 1 + (5/16.0), 3.3, 35),
                new PunchPrice(1 + (9/32.0), 1 + (3/8.0), 3.3, 35),
                new PunchPrice(1 + (19/64.0), 1 + (5/16.0), 3.3, 35),
                new PunchPrice(1 + (5/16.0), 1 + (3/8.0), 3.3, 35),            
                new PunchPrice(1 + (21/64.0), 1 + (3/8.0), 3.9, 35),
                new PunchPrice(1 + (21/64.0), 1 + (7/16.0), 3.3, 35),
                new PunchPrice(1 + (11/32.0), 1 + (3/8.0), 3.3, 35),
                new PunchPrice(1 + (23/64.0), 1 + (3/8.0), 3.3, 35),
                new PunchPrice(1 + (3/8.0), 1 + (7/16.0), 3.9, 35),
                new PunchPrice(1 + (25/64.0), 1 + (7/16.0), 3.9, 35),
                new PunchPrice(1 + (13/32.0), 1 + (7/16.0), 3.9, 35),
                new PunchPrice(1 + (27/64.0), 1 + (7/16.0), 3.9, 35),
                new PunchPrice(1 + (7/16.0), 1 + (1/2.0), 4.1, 35),
                new PunchPrice(1 + (29/64.0), 1 + (1/2.0), 4.1, 35),
                new PunchPrice(1 + (29/64.0), 1 + (9/16.0), 4.75, 35),
                new PunchPrice(1 + (31/64.0), 1 + (1/2.0), 4.1, 35),
                new PunchPrice(1 + (1/2.0), 1 + (9/16.0), 4.75, 35),
                new PunchPrice(1 + (33/64.0), 1 + (9/16.0), 4.75, 35),
                new PunchPrice(1 + (33/64.0), 1 + (5/8.0), 5.1, 35),
                new PunchPrice(1 + (17/32.0), 1 + (9/16.0), 4.75, 35),
                new PunchPrice(1 + (9/16.0), 1 + (5/8.0), 5.1, 35),
                new PunchPrice(1 + (37/64.0), 1 + (5/8.0), 5.1, 35),
                new PunchPrice(1 + (19/32.0), 1 + (5/8.0), 5.1, 35),
                new PunchPrice(1 + (39/64.0), 1 + (5/8.0), 5.1, 35),
                new PunchPrice(1 + (5/8.0), 1 + (11/16.0), 11.9, 35),
                new PunchPrice(1 + (41/64.0), 1 + (3/4.0), 12.3, 35),
                new PunchPrice(1 + (11/16.0), 1 + (3/4.0), 12.3, 35),
                new PunchPrice(1 + (23/32.0), 1 + (3/4.0), 12.3, 35),            
            }; 
            
            return tubePunchList;
        }

        private List<PunchPrice> GetHdsoPunchList()
        {
            var hdsoPunchList = new List<PunchPrice>()
            {
                new PunchPrice(1/16.0, 5/16.0, 6.5, 9.0),   // 1/16 Cut
                new PunchPrice(7/64.0, 5/16.0,6.5, 9.0),   // 7/64 Cut
                new PunchPrice(1/8.0, 5/16.0,6.5, 9.0),   // 1/8 Cut
                new PunchPrice(9/64.0,5/16.0, 6.5, 9.0),   // 9/64 Cut
                new PunchPrice(5/32.0, 5/16.0,6.5, 9.0),   // 5/32 Cut
                new PunchPrice(11/64.0, 5/16.0,6.5, 9.0),   // 11/64 Cut
                new PunchPrice(3/16.0, 3/8.0,6.5, 9.0),   // 3/16 Cut
                new PunchPrice(13/64.0, 3/8.0,6.5, 9.0),   // 13/64 Cut
                new PunchPrice(7/32.0, 3/8.0,6.5, 9.0),   // 7/32 Cut
                new PunchPrice(15/64.0, 3/8.0,6.5, 9.0),   // 15/64 Cut
                new PunchPrice(1/4.0, 3/8.0,8.0, 9.0),      // 1/4 Cut
                new PunchPrice(5/16.0, 1/2.0,8.0, 9.0),     // 5/16 Cut
                new PunchPrice(3/8.0, 1/2.0,8.0, 9.0),     // 3/8 Cut
                new PunchPrice(1/2.0, 5/8.0,8.0, 9.0)        // 1/2 Cut
            };
            return hdsoPunchList;
        }

        private List<PunchPrice> GetFtPunchList()
        {
            var ftPunchList = new List<PunchPrice>()
            {
                new PunchPrice(1/32.0, 1/8.0,8.5, 3),   // 1/32 Cut, 1/8 Base
                new PunchPrice(1/32.0, 3/16.0,8.5, 3),   // 1/32 Cut, 3/16 Base
                new PunchPrice(3/64.0, 1/8.0,8.5, 3),   // 3/64 Cut, 1/8 Base
                new PunchPrice(1/16.0, 1/8.0,8.5, 3),   // 1/16 Cut, 1/8 Base
                new PunchPrice(1/16.0, 3/16.0,8.5, 3),   // 1/16 Cut, 3/16 Base
                new PunchPrice(5/64.0, 3/16.0,8.5, 3),   // 5/64 Cut, 3/16 Base
                new PunchPrice(3/32.0, 1/16.0,4, 3),     // 3/32 Cut, 3/16 Base
                new PunchPrice(7/64.0, 3/16.0,4, 3),     // 7/64 Cut, 3/16 Base
                new PunchPrice(1/8.0, 3/16.0,4, 3),     // 1/8 Cut, 3/16 Base
                new PunchPrice(1/8.0, 1/4.0,4, 3),     // 1/8 Cut, 1/4 Base
                new PunchPrice(9/64.0, 1/4.0,4, 3),     // 9/64 Cut, 1/4 Base
                new PunchPrice(5/32.0, 1/4.0,4, 3),     // 5/32 Cut, 1/4 Base
                new PunchPrice(11/64.0, 1/4.0,4, 3),     // 11/64 Cut, 1/4 Base
                new PunchPrice(3/16.0, 1/4.0,4, 3),     // 3/16 Cut, 1/4 Base
                new PunchPrice(3/16.0, 5/16.0,4, 3),     // 3/16 Cut, 5/16 Base
                new PunchPrice(13/64.0, 5/16.0,4, 3),     // 13/64 Cut, 5/16 Base
                new PunchPrice(7/32.0, 5/16.0,4, 3),     // 7/32 Cut, 5/16 Base
                new PunchPrice(15/64.0, 5/16.0,4, 3),     // 15/64 Cut, 5/16 Base
                new PunchPrice(1/4.0, 5/16.0,4, 3),     // 1/4 Cut, 5/16 Base
                new PunchPrice(1/4.0, 3/8.0,4, 3),     // 1/4 Cut, 3/8 Base
                new PunchPrice(17/64.0,3/8.0 ,4, 3),     // 17/64 Cut, 3/8 Base
                new PunchPrice(9/32.0, 3/8.0,4, 3),     // 9/32 Cut, 3/8 Base
                new PunchPrice(19/64.0, 3/8.0,4, 3),     // 19/64 Cut, 3/8 Base
                new PunchPrice(5/16.0, 3/8.0,4, 3),     // 5/16 Cut, 3/8 Base
                new PunchPrice(5/16.0, 7/16.0,4, 3),     // 5/16 Cut, 7/16 Base
                new PunchPrice(21/64.0, 7/16.0,4, 3),     // 21/64 Cut, 7/16 Base
                new PunchPrice(11/32.0, 7/16.0,4, 3),     // 11/32 Cut, 7/16 Base
                new PunchPrice(23/64.0, 7/16.0,4, 3),     // 23/64 Cut, 7/16 Base
                new PunchPrice(3/8.0, 7/16.0,4, 3),     // 3/8 Cut, 7/16 Base
                new PunchPrice(3/8.0, 1/2.0,4, 3),     // 3/8 Cut, 1/2 Base
                new PunchPrice(25/64.0, 1/2.0,4, 3),     // 25/64 Cut, 1/2 Base
                new PunchPrice(13/32.0, 1/2.0,4, 3),     // 13/32 Cut, 1/2 Base
                new PunchPrice(27/64.0, 1/2.0,4, 3),     // 27/64 Cut, 1/2 Base
                new PunchPrice(7/16.0, 1/2.0,4, 3),     // 7/16 Cut, 1/2 Base
                new PunchPrice(7/16.0, 9/16.0,4, 3),     // 7/16 Cut, 9/16 Base
                new PunchPrice(29/64.0, 9/16.0,4, 3),     // 29/64 Cut, 9/16 Base
                new PunchPrice(15/32.0, 9/16.0,4, 3),     // 15/32 Cut, 9/16 Base
                new PunchPrice(31/64.0, 9/16.0,4, 3),     // 31/64 Cut, 9/16 Base
                new PunchPrice(1/2.0, 9/16.0,4, 3),     // 1/2 Cut, 9/16 Base
                new PunchPrice(1/2.0, 5/8.0,4, 3),     // 1/2 Cut, 5/8 Base
                new PunchPrice(33/64.0, 5/8.0,4, 3),     // 33/64 Cut, 5/8 Base
                new PunchPrice(17/32.0, 5/8.0,4, 3),     // 17/32 Cut, 5/8 Base
                new PunchPrice(35/64.0, 5/8.0,4, 3),     // 35/64 Cut, 5/8 Base
                new PunchPrice(9/16.0, 11/16.0,4, 3),     // 9/16 Cut, 11/16 Base
                new PunchPrice(37/64.0, 11/16.0,4, 3),     // 37/64 Cut, 11/16 Base
                new PunchPrice(19/32.0, 11/16.0,4, 3),     // 19/32 Cut, 11/16 Base
                new PunchPrice(39/64.0, 11/16.0,4, 3),     // 39/64 Cut, 11/16 Base
                new PunchPrice(5/8.0, 3/4.0,4, 3),     // 5/8 Cut, 3/4 Base
                new PunchPrice(41/64.0, 3/4.0,4, 3),     // 41/64 Cut, 3/4 Base
                new PunchPrice(21/32.0, 3/4.0,4, 3),     // 21/32 Cut, 3/4 Base
                new PunchPrice(11/16.0, 13/16.0,4, 3),     // 11/16 Cut, 13/16 Base
                new PunchPrice(45/64.0, 13/16.0,4, 3),     // 45/64 Cut, 13/16 Base
                new PunchPrice(23/32.0, 13/16.0,4, 3),     // 23/32 Cut, 13/16 Base
                new PunchPrice(3/4.0, 7/8.0,4, 3),     // 3/4 Cut, 7/8 Base
                new PunchPrice(49/64.0, 7/8.0,4, 3),     // 49/64 Cut, 7/8 Base
                new PunchPrice(25/32.0, 7/8.0,4, 3),     // 25/32 Cut, 7/8 Base
                new PunchPrice(13/16.0, 15/16.0,6, 3),     // 13/16 Cut, 15/16 Base
                new PunchPrice(53/64.0, 15/16.0,6, 3),     // 53/64 Cut, 15/16 Base
                new PunchPrice(27/32.0, 15/16.0,6, 3),     // 27/32 Cut, 15/16 Base
                new PunchPrice(7/8.0, 1.0,6, 3),     // 7/8 Cut, 1 Base
                new PunchPrice(57/64.0, 1.0,6, 3),     // 57/64 Cut, 1 Base
                new PunchPrice(29/32.0, 1.0,6, 3),     // 29/32 Cut, 1 Base
                new PunchPrice(15/16.0, 1.0 + (1/16.0),6, 3),     // 15/16 Cut, 1-1/16 Base
                new PunchPrice(1.0, 1.0 + (1/8.0),6, 3),     // 1 Cut, 1-1/8 Base
                new PunchPrice(1.0 + (1/64.0), 1.0 + (1/8.0),15, 3),    // 1-1/64 Cut, 1-1/8 Base
                new PunchPrice(1.0 + (1/32.0), 1.0 + (1/8.0),15, 3),    // 1-1/32 Cut, 1-1/8 Base
                new PunchPrice(1.0 + (1/8.0), 1.0 + (1/4.0),15, 3),    // 1-1/8 Cut, 1-1/4 Base
                new PunchPrice(1.0 + (3/16.0), 1.0 + (5/16.0),15, 3),    // 1-3/16 Cut, 1-5/16 Base
                new PunchPrice(1.0 + (3/8.0), 1.0 + (1/2.0),15, 3)     // 1-3/8 Cut, 1-1/2 Base
            };
            return ftPunchList;
        }

        private List<PunchPrice> GetSoPunchList()
        {
            List<PunchPrice> soPunchList = new List<PunchPrice>()
            {
                new PunchPrice(1/32.0, 1/8.0, 6.45, 4),
                new PunchPrice(1/32.0, 3/16.0, 5.5, 4),
                new PunchPrice(3/64.0, 1/8.0, 6.45, 4),
                new PunchPrice(3/64.0, 3/16.0, 5.5, 4),
                new PunchPrice(1/16.0, 1/8.0, 5.5, 4),
                new PunchPrice(1/16.0, 3/16.0, 4.25, 4),
                new PunchPrice(5/64.0, 1/8.0, 11.75, 4),
                new PunchPrice(5/64.0, 3/16.0, 4.25, 4),
                new PunchPrice(3/32.0, 3/16.0, 3.8, 4)             ,
                new PunchPrice(3/32.0, 1/4.0, 3.8, 7)            ,
                new PunchPrice(7/64.0, 3/16.0, 3.8, 4)            ,
                new PunchPrice(7/64.0, 1/4.0, 3.8, 7),
                new PunchPrice(1/8.0, 1/4.0, 3.8, 7),
                new PunchPrice(1/8.0, 5/16.0, 3.9, 7),
                new PunchPrice(9/64.0, 1/4.0, 3.9, 7),            
                new PunchPrice(9/64.0, 5/16.0, 3.9, 7),
                new PunchPrice(5/32.0, 1/4.0, 3.9, 7),            
                new PunchPrice(5/32.0, 5/16.0, 3.9, 7),
                new PunchPrice(11/64.0 ,1/4.0, 5.05, 7),
                new PunchPrice(11/64.0, 5/16.0, 3.9, 7),
                new PunchPrice(3/16.0, 5/16.0, 3.9, 7),
                new PunchPrice(3/16.0, 3/8.0, 5.75, 7),
                new PunchPrice(13/64.0, 5/16.0, 3.9, 7),
                new PunchPrice(13/64.0, 3/8.0, 3.9, 7),
                new PunchPrice(7/32.0, 5/16.0, 4, 7),
                new PunchPrice(7/32.0, 3/8.0, 4, 7),
                new PunchPrice(15/64.0, 3/8.0, 4, 7),
                new PunchPrice(1/4.0, 3/8.0, 4, 7),
                new PunchPrice(17/64.0, 3/8.0, 5.75, 7),
                new PunchPrice(9/32.0, 7/16.0, 4, 7),
                new PunchPrice(9/32.0, 3/8.0, 5.75, 7),
                new PunchPrice(19/64.0, 7/16.0, 4, 7),
                new PunchPrice(5/16.0, 7/16.0, 4, 7),            
                new PunchPrice(21/64.0, 7/16.0, 5.95, 7),
                new PunchPrice(21/64.0, 1/2.0, 4.99, 7),
                new PunchPrice(11/32.0, 1/2.0, 4.55, 7),
                new PunchPrice(11/32.0, 9/16.0, 7.55, 7),
                new PunchPrice(23/64.0, 1/2.0, 4.55, 7),
                new PunchPrice(3/8.0, 1/2.0, 4.55, 7),
                new PunchPrice(3/8.0, 9/16.0, 7.55, 7),
                new PunchPrice(3/8.0, 5/8.0, 8, 7),
                new PunchPrice(25/64.0, 1/2.0, 8.25, 7),
                new PunchPrice(25/64.0, 9/16.0, 7.55, 7), 
                new PunchPrice(11/32.0, 1/2.0, 8.25, 7),
                new PunchPrice(11/32.0, 9/16.0, 7.55, 7),
                new PunchPrice(13/32.0, 5/8.0, 8, 7),
                new PunchPrice(27/64.0, 9/16.0, 7.55, 7),
                new PunchPrice(27/64.0, 5/8.0, 8, 7),
                new PunchPrice(7/16.0, 9/16.0, 7.55, 7),
                new PunchPrice(7/16.0, 5/8.0, 6.15, 7),
                new PunchPrice(29/64.0, 5/8.0, 8, 7),
                new PunchPrice(15/32.0, 5/8.0, 6.15, 7),
                new PunchPrice(15/32.0, 3/4.0, 14.05, 7),
                new PunchPrice(31/64.0, 5/8.0, 8, 7),
                new PunchPrice(1/2.0, 5/8.0, 6.15, 7),
                new PunchPrice(1/2.0, 3/4.0, 12.2, 7),
                new PunchPrice(17/32.0, 3/4.0, 12.2, 7),            
                new PunchPrice(9/16.0, 3/4.0, 12.2, 7),
                new PunchPrice(37/64.0, 3/4.0, 14.05, 7),
                new PunchPrice(39/64.0, 3/4.0, 14.05, 7),
                new PunchPrice(5/8.0, 7/8.0, 16, 7),            
            }; 
            
            return soPunchList;
        }

        private List<PunchPrice> GetSwPunchList()
        {
            var swList = new List<PunchPrice>()
            {
                new PunchPrice(5/64.0, 5/64.0, 7.8, 3),
                new PunchPrice(3/32.0, 32.0, 7.1, 3),
                new PunchPrice(7/64.0, 7/64.0, 6.45, 3),
                new PunchPrice(1/8.0, 9/64.0, 6.45, 3),
                new PunchPrice(9/64.0, 5/32.0, 6, 3),
                new PunchPrice(5/32.0, 11/64.0, 6.35, 3),
                new PunchPrice(11/64.0, 3/16.0, 5, 3),
                new PunchPrice(3/16.0, 13/64.0, 6, 3),
                new PunchPrice(13/64.0, 7/32.0, 5.95, 3),
                new PunchPrice(7/32.0, 15/64.0, 6.05, 3),
                new PunchPrice(15/64.0, 1/4.0, 3, 3),
                new PunchPrice(1/4.0, 17/64.0, 6, 3),
                new PunchPrice(17/64.0, 9/32.0, 6.1, 3),
                new PunchPrice(9/32.0, 16/64.0, 6.2, 3),
                new PunchPrice(19/64.0, 5/16.0, 3.25, 4),
                new PunchPrice(5/16.0, 21/64.0, 6.2, 3),
                new PunchPrice(21/64.0, 11/32.0, 6, 3),
                new PunchPrice(11/32.0, 23/64.0, 6.1, 3),
                new PunchPrice(23/64.0, 3/8.0, 3.25, 4),
                new PunchPrice(3/8.0, 25/64.0, 6.5, 3),
                new PunchPrice(25/64.0, 13/32.0, 6.2, 3),
                new PunchPrice(13/32.0, 27/64.0, 6.5, 3),
                new PunchPrice(27/64.0, 7/16.0, 3.25, 4),
                new PunchPrice(7/16.0, 29/64.0, 6.7, 3),
                new PunchPrice(29/64.0, 15/32.0, 6.35, 3),
                new PunchPrice(15/32.0, 31/64.0, 6.7, 3),
                new PunchPrice(31/64.0, 1/2.0, 3.25, 4),            
                new PunchPrice(1/2.0, 33/64.0, 6.85, 3),  
                new PunchPrice(33/64.0, 17/32.0, 8, 3),  
            }; 
            
            return swList;
        }
        
        private List<PunchPrice> GetRetractList()
        {
            var retractList = new List<PunchPrice>()
            {
                new PunchPrice(1/8.0, 1/4.0, 8, 4),
                new PunchPrice(1/4.0, 3/8.0, 11, 4),
            };
             
            return retractList;
        }
        
    }
}
