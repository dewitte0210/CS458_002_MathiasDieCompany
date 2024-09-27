/*
 * Abstract class meant to be inherited by every child feature class (ex. Circle)
 * for DWG and PDF files the features will be recognized based on the entities within
 */
using System;
using System.IO;
using System.Numerics;

abstract public class Feature
{
    PossibleFeatureTypes featureType;
    Entity[] entityList;
    bool kissCut;
    bool multipleRadius;
    bool perOver20;
    protected enum PossibleFeatureTypes
    {
        rectangle,
        circle,
    }
    
    public Feature()
    {
        kissCut = false;
        multipleRadius = false;
        perOver20 = false;
    }

    abstract public void calcPerimeter();
}
