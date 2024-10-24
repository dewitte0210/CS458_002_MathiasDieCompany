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
    List<Entity> entityList; //list of touching entities that make up the feature
    [JsonProperty]
    bool kissCut;
    [JsonProperty]
    bool multipleRadius;
    [JsonProperty]
    double perimeter;
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

    public Feature(string featureType, bool kissCut, bool multipleRadius, bool border)
    {
        this.count = 1;
        //change string input to enum value
        PossibleFeatureTypes inputAsEnum = (PossibleFeatureTypes)Enum.Parse(typeof(PossibleFeatureTypes), featureType);
        this.featureType = inputAsEnum;
        this.kissCut = kissCut;
        this.multipleRadius = multipleRadius;
        this.border = border;

        calcPerimeter();
    }

    public Feature(List<Entity> entityList)
    {
        this.count = 1;
        this.entityList = entityList;

        int numLines = 0;
        int numArcs = 0;
        int numCircles = 0;

        //count the number of each entity type
        for (int i = 0; i < entityList.Count; i++)
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

        //check two conditions possible to make Group1B (with no perimeter features)
        if (numCircles == 1 || (numLines == 2 && numArcs == 2))
        {
            featureType = PossibleFeatureTypes.Group1B;
        }
        //check two conditions possible to make Group1A (with no perimeter features)
        else if (numLines == 4 && (numArcs == 0 || numArcs == 4))
        {
            featureType = PossibleFeatureTypes.Group1A;
        }
        else
        {
            Console.WriteLine("Error: Cannot assign feature type.");
        }

        //calculate and set the perimeter of the feature
        calcPerimeter();
    }

    //calculates the perimeter of the feature
    public void calcPerimeter()
    {
        for (int i = 0; i < entityList.Count; i++)
        {
            perimeter += entityList[i].getLength();
        }
    }

    /*
     * Overriding the Equals method to compare two Feature objects
    */
    public override bool Equals(object obj)
    {
        var item = obj as Feature;
        if (item == null)
        {
            return false;
        }

        //calculate difference in order to use tolerence
        double perDiff = perimeter - item.perimeter;

        //if the features are identical Group1B features
        if (featureType == item.featureType && featureType == PossibleFeatureTypes.Group1B && kissCut == item.kissCut && multipleRadius == item.multipleRadius &&
                Math.Abs(perDiff) < 0.0005 && border == item.border)
        {
            //may need to change in the case where a circle is made of two arcs seperated by perimeter features
            if (entityList.Count != item.entityList.Count)
            {
                return false;
            }
            //return true if they are identical non-circular Group1B features
            if (entityList.Count == 4 && item.entityList.Count == 4)
            {
                return true;
            }
            //if they are circular
            else if (entityList.Count == 1 && item.entityList.Count == 1)
            {
                //serialize and deserialize in order to set them to circle objects, should look into different way of doing this
                //TODO: create added check if they are arcs instead of circles
                var serializedParent = JsonConvert.SerializeObject(entityList[0]);
                Circle c1 = JsonConvert.DeserializeObject<Circle>(serializedParent);
                serializedParent = JsonConvert.SerializeObject(item.entityList[0]);
                Circle c2 = JsonConvert.DeserializeObject<Circle>(serializedParent);

                if (c1.radius == c2.radius)
                {
                    return true;
                }
            }
        }

        // Checking equality of all other feature types
        else if (featureType == item.featureType && kissCut == item.kissCut && multipleRadius == item.multipleRadius &&
                Math.Abs(perDiff) < 0.0005 && border == item.border)
        {
            return true;
        }
        //not equal
        return false;
    }

}