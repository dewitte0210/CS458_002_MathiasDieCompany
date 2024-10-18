/*
 * For borders look at every feature that is not inside another feature (that is a border)
 * calc number up using number of borders
 * 
 * for optimization before detecting features ignore all entity groups outside
 * the first border and calculates feautrues only for that one 
 */
using FeatureRecognitionAPI.Models;
using iText.StyledXmlParser.Node;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Numerics;

public class Feature
{
    [JsonProperty]
    PossibleFeatureTypes featureType;
    [JsonProperty]
    Entity[] entityList; //list of touching entities that make up the feature
    [JsonProperty]
    bool kissCut;
    [JsonProperty]
    bool multipleRadius;
    [JsonProperty]
    bool perOver20;
    [JsonProperty]
    bool border;
    public int count;
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    protected enum PossibleFeatureTypes
    {
        [JsonProperty]
        Group1A,
        Group1B,
        Group3,
        Group1C,
        Group6,
        Group2A
    }

    private Feature() { }//should not use default constructor

    public Feature(string featureType, bool kissCut, bool multipleRadius, bool perOver20, bool border)
    {
        this.count = 1;
        PossibleFeatureTypes inputAsEnum = (PossibleFeatureTypes)Enum.Parse(typeof(PossibleFeatureTypes), featureType);
        this.featureType = inputAsEnum;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;
        this.perOver20 = perOver20;
        this.border = border;
    }

    public Feature(Entity[] entityList)
    {
        this.count = 1;
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

    /*
     * Overriding the Equals method to compare two Feature objects
    */
    public override bool Equals(object obj)
    {
        var item = obj as Feature;

        /*
            TO BE IMPLEMENTED LATER
        */
        //if (featureType == PossibleFeatureTypes.Group1B && entityList.Length == 1)
        //{
        //    //add check, could be arc

        //    var serializedParent = JsonConvert.SerializeObject(entityList[0]);
        //    Circle c1 = JsonConvert.DeserializeObject<Circle>(serializedParent);
        //    serializedParent = JsonConvert.SerializeObject(item.entityList[0]);
        //    Circle c2 = JsonConvert.DeserializeObject<Circle>(serializedParent);

        //    if (kissCut == item.kissCut && multipleRadius == item.multipleRadius &&
        //        perOver20 == item.perOver20 && border == item.border && c1.radius == c2.radius)
        //    {
        //        return true;
        //    }
        //}
        //else

        // Checking equality
        if (featureType == item.featureType && kissCut == item.kissCut && multipleRadius == item.multipleRadius &&
                perOver20 == item.perOver20 && border == item.border)
        {
            return true;
        }
        return false;
    }

}