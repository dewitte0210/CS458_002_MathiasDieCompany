﻿using System.Globalization;
using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using FeatureRecognitionAPI.Models.Pricing;
using Newtonsoft.Json;

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
        private readonly IPricingDataService _dataService; 
        public PricingService(IPricingDataService dataService)
        {
            _dataService = dataService;
            _featurePriceList = _dataService.GetFeaturePrices();
            
            PunchPriceReturn punches = _dataService.GetPunchPrices();
            _tubePunchList = punches.TubePunchList;
            _soPunchList = punches.SoPunchList;
            _hdsoPunchList = punches.HdsoPunchList;
            _ftPunchList = punches.FtPunchList;
            _swPunchList = punches.SwPunchList;
            _retractList = punches.RetractList;
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
       
        }
}
