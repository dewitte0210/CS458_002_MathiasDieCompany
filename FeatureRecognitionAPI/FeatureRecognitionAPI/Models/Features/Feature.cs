﻿/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 * 
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates feautrues only for that one 
 */
using FeatureRecognitionAPI.Models;
using System;
using System.IO;
using System.Numerics;

public class Feature
{
    PossibleFeatureTypes featureType;
    Entity[] entityList; //list of touching entities that make up the feature
    bool kissCut;
    bool multipleRadius;
    bool perOver20;
    bool border;
    protected enum PossibleFeatureTypes
    {
        Group1A,
        Group1B,
        Group3,
        Group1C,
        Group6,
        Group2A
    }

    private Feature() { }//should not use default constructor

    public Feature(Entity[] entityList)
    {
        this.entityList = entityList;

        int numLines = 0;
        int numArcs = 0;
        int numCircles = 0;

        for (int i = 0; i < entityList.Length; i++)
        {
            if (entityList[i] is Line)
            {
                numLines++;
            }
            else if (entityList[i] is Arc)
            {
                numArcs++;
            }
            else if (entityList[i] is Circle)
            {
                numCircles++;
            }
            else
            {
                Console.WriteLine("Error: Cannot detect entity type.");
                break;
            }
        }
        //Console.WriteLine(numLines + " " + numArcs + " " + numCircles);

        if (numCircles == 1 || (numLines == 2 && numArcs == 2))
        {
            featureType = PossibleFeatureTypes.Group1B;
        }
        else if (numLines == 4 && (numArcs == 0 || numArcs ==4))
        {
            featureType = PossibleFeatureTypes.Group1A;
        }
        else
        {
            Console.WriteLine("Error: Cannot assign feature type.");
        }
    }

    //calculates if the perimeter is over 20
    public void calcPerimeter()
    {
        double sum = 0;
        for (int i = 0; i < entityList.Length; i++)
        {
            sum += entityList[i].getLength();
        }
        if (sum > 20)
        {
            perOver20 = true;
        }
        else
        {
            perOver20 = false;
        }
    }


}