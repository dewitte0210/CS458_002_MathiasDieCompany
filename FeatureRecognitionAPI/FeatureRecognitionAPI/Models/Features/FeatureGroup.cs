using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Features;

public class FeatureGroup
{
    // todo: just make Count return length of feature list
    //Track how many feature groups of this type are found
    public int NumIdenticalFeatureGroups { get; set; }
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
        List<Feature> featuresToAdd = new();
        foreach (Feature feature in features)
        {
            if (feature.ChamferList.Count <= 0) continue;

            List<Line> chamfersToRemove = new();
            List<Entity> newEntityList = feature.EntityList.ToList();
            foreach (ChamferGroup cg in feature.ChamferList)
            {
                // make new chamfer feature
                featuresToAdd.Add(new Feature(PossibleFeatureTypes.Group3, [new Line(cg.Chamfer)]));
                
                // add to a new list so that the indexes in entity list stay the same
                chamfersToRemove.Add(cg.Chamfer);

                // todo: decide whether to remove the chamfers because it shows up in front end weird
                //EntityTools.ExtendTwoLines(newEntityList[cg.LineAIndex] as Line,
                //    newEntityList[cg.LineBIndex] as Line);
            }

            // actually remove the chamfers
            //foreach (Line chamfer in chamfersToRemove)
            //{
            //    newEntityList.Remove(chamfer);
            //}
            feature.ChamferList.Clear();
            chamfersToRemove.Clear();
            feature.EntityList = newEntityList;
        }
        // add the new chamfer features to the group
        features.AddRange(featuresToAdd);
        NumIdenticalFeatureGroups += featuresToAdd.Count;
        featuresToAdd.Clear();
    }

    public void FindFeatureTypes()
    {
        List<Feature> featToAdd = new List<Feature>();
        foreach (Feature feature in features)
        {

            feature.ExtendAllEntities();
            feature.SeparateBaseEntities();
            feature.SeparatePerimeterEntities();
            feature.DetectFeatures();
            
            foreach (Feature perimeterFeature in feature.PerimeterFeatureList)
            {
                featToAdd.Add(perimeterFeature);
            }
        }
        features.AddRange(featToAdd);
        
        // break out chamfers
        BreakOutChamfers();

        // Group identical features together
        for (int i = 0; i < features.Count; i++)
        {
            //ignore group 3 chamfer check because equals sees them as the same even though they are not
            if (features[i].FeatureType == PossibleFeatureTypes.Group3)
            {
                continue;
            }
            
            for (int j = i + 1; j < features.Count; j++)
            {
                if (features[i].Equals(features[j]))
                {
                    features[i].Count += features[j].Count;
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
        
    /// <summary>
    /// Check of all features in the group have a corresponding feature in other group, return true if they do 
    /// </summary>
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
        features.Sort((x, y) => x.Perimeter.CompareTo(y.Perimeter));
        ((FeatureGroup)obj).features.Sort((x, y) => x.Perimeter.CompareTo((y.Perimeter)));


        for (int i = 0; i < features.Count; i++)
        {
            //While this features @ i has same perimeter as obj.features[j] check if any j = features[i]
            int j = i;
            bool checkPoint = false;
            while (j < features.Count
                   && Math.Abs(features[i].Perimeter - ((FeatureGroup)obj).features[j].Perimeter) <
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