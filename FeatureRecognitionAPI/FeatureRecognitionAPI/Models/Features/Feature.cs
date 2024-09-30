/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 * 
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates feautrues only for that one 
 */
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
    
    public Feature()
    {
        kissCut = false;
        multipleRadius = false;
        perOver20 = false;
        border = false;
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
