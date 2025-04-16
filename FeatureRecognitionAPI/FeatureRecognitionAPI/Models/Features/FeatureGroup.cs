using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;
using NHibernate.Event.Default;

namespace FeatureRecognitionAPI.Models.Features;

public class FeatureGroup
{
    public int Count { get; set; } //Track how many feature groups of this type are found
    private int totalArcs;
    private int totalLines;
    private int totalCircles;
    [JsonProperty] protected List<Feature> features;

    public FeatureGroup(List<Feature> features)
    {
        // this.count = count;
        this.features = features;

        foreach (Feature feature in features)
        {
            this.totalArcs += feature.GetNumArcs();
            this.totalLines += feature.GetNumLines();
            this.totalCircles += feature.GetNumCircles();
        }
    }
    
    private void BreakOutChamfers()
    {
        // todo: extend the two lines the chamfer was touching to clean up the shape
        // todo: make so it works with more than one chamfer
        // todo: just make Count return length of feature list
            
        List<Feature> featuresToAdd = new();
        foreach (Feature feature in features)
        {
            if (feature.ChamferList.Count <= 0) continue;

            List<Entity> linesToExtend = new();
                
            foreach (ChamferGroup cg in feature.ChamferList)
            {
                // make new chamfer feature
                featuresToAdd.Add(new Feature(PossibleFeatureTypes.Group3, [cg.Chamfer]));
                Count++;
                    
                // todo: check if not removing breaks group1A and entity count numbers
                // remove chamfer from original feature
                if (feature.EntityList.Remove(cg.Chamfer))
                {
                    linesToExtend.Add(cg.LineA);
                    linesToExtend.Add(cg.LineB);
                }
            }
            feature.ChamferList.Clear();

            // step every 2 because the lines touching chamfers come in pairs
            // because there are duplicates in the real line list
            // all lines that need to be extended will be
            for (int i = 0; i < linesToExtend.Count; i += 2)
            {
                Line realLine1 = feature.EntityList.Find(linesToExtend[i]) as Line;
                Line realLine2 = feature.EntityList.Find(linesToExtend[i + 1]) as Line;
                
                if (realLine1 == null || realLine2 == null) continue;
                
                //directly extends lines from entity list
                EntityTools.ExtendTwoLines(realLine1, realLine2);
            }
        }
        features.AddRange(featuresToAdd);
        featuresToAdd.Clear();
    }

    public void FindFeatureTypes()
    {
        for (int i = 0; i < features.Count; i++)
        {
            features[i].ExtendAllEntities();
            features[i].SeperateBaseEntities();
            features[i].SeperatePerimeterEntities();
            features[i].DetectFeatures();
            if (features[i].PerimeterEntityList != null)
            {
                for (int j = 0; j < features[i].PerimeterEntityList.Count; j++)
                {
                    Feature newFeat = new(features[i].PerimeterEntityList[j]);
                    newFeat.DetectFeatures();
                    features.Add(newFeat);
                }
            }
        }

        // break out chamfers
        BreakOutChamfers();

        // Group identical features together
        for (int i = 0; i < features.Count; i++)
        {
            for (int j = i + 1; j < features.Count; j++)
            {
                //ignore group 3 chamfer check because equals sees them as the same even though they are not
                if (features[i].Equals(features[j]) && features[i].FeatureType != PossibleFeatureTypes.Group3)
                {
                    features[i].count += features[j].count;
                    features.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    public void SetFeatureList(List<Feature> features)
    {
        this.features = features;
    }

    public List<Feature> GetFeatures()
    {
        return this.features;
    }
        
    //Check of all features in the group have a corresponding feature in other group, return true if they do
    public override bool Equals(object? obj)
    {
        if (!(obj is FeatureGroup) || obj == null)
        {
            return false;
        }

        if (obj == this)
        {
            return true;
        }

        //If here, obj is a FeatureGroup
        if (!(this.totalArcs == ((FeatureGroup)obj).totalArcs
              && this.totalLines == ((FeatureGroup)obj).totalLines
              && this.totalCircles == ((FeatureGroup)obj).totalCircles
            )) {
            return false;
        }

        //Sort by perimeter
        features.Sort((x, y) => x.perimeter.CompareTo(y.perimeter));
        ((FeatureGroup)obj).features.Sort((x, y) => x.perimeter.CompareTo((y.perimeter)));


        for (int i = 0; i < features.Count; i++)
        {
            //While this features @ i has same perimeter as obj.features[j] check if any j = features[i]
            int j = i;
            bool checkPoint = false;
            while (j < features.Count
                   && Math.Abs(features[i].perimeter - ((FeatureGroup)obj).features[j].perimeter) <
                   Entity.EntityTolerance)
            {
                if (features[i].Equals(features[j]))
                {
                    checkPoint = true;
                    break;
                }

                //If first element checked isn't equal, increment J
                j++;
            }

            if (!checkPoint)
            {
                return false;
            }
        }

        //If we got here, checkPoint was never false, so return true;
        return true;
    }
        
    /* TODO: complete this function to replace the one above. It will fix a few cases that the above doesnt catch
     * //Check of all features in the group have a corresponding feature in other group, return true if they do
    public override bool Equals(object? obj)
    {
        if (!(obj is FeatureGroup objF) || obj == null)
        {
            return false;
        }

        if (obj == this)
        {
            return true;
        }

        //If here, obj is a FeatureGroup
        if (!(this.totalArcs == objF.totalArcs
              && this.totalLines == objF.totalLines
              && this.totalCircles == objF.totalCircles
            )) {
            return false;
        }

        bool[][] elementMatch = new bool[2][features.Count]; // first index 0 is for features, 1 is for objF.features
        elementMatch.Add(new List<bool>(features.Count));
        elementMatch.Add(new List<bool>(features.Count));
        for (int i = 0; i < features.Count; i++)
        {
            for (int j = 0; j < objF.features.Count; j++)
            {
                if (!elementMatch[1][j] && features[i].Equals(objF.features[j]))
                {
                    elementMatch[0][i] = true;
                    elementMatch[1][j] = true;
                }
            }
            if(!elementMatch[0][i]) {return false;}
        }

        if (elementMatch[0].Contains(false)) {return false;}
        if (elementMatch[1].Contains(false)) {return false;}
        return true;

    }*/
}