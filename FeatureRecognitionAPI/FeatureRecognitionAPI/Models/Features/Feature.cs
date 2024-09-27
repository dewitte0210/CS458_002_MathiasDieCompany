/*
 * Abstract class meant to be inherited by every child feature class (ex. Circle)
 * for DWG and PDF files the features will be recognized based on the entities within
 */
using System;
using System.IO;
using System.Numerics;

namespace FeatureRecognitionAPI.Models
{
    abstract public class Feature
    {
        PossibleFeatureTypes featureType;
        protected enum PossibleFeatureTypes
        {
            rectangle,
            circle,
        }

        public Feature()
        {

        }
    }
}
