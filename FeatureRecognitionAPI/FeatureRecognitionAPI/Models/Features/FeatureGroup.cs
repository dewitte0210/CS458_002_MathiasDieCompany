using FeatureRecognitionAPI.Models.Entities;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Models.Utility;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Features;

public class FeatureGroup
{
    // Track how many feature groups of this type are found.
    // Don't rename Count unless you change it in the front end too.
    public int Count { get; set; }
    private readonly int _totalArcs;
    private readonly int _totalLines;
    private readonly int _totalCircles;

    // Don't rename unless you change the front end as well.
    [JsonProperty] protected internal List<Feature> features { get; set; }

    public FeatureGroup(List<Feature> featureList)
    {
        features = featureList;

        foreach (Feature feature in features)
        {
            _totalArcs += feature.GetNumArcs();
            _totalLines += feature.GetNumLines();
            _totalCircles += feature.GetNumCircles();
        }
    }

    /// <summary>
    /// After feature detection, chamfered lines are only flagged and should be their own feature.
    /// This function breaks those flagged chamfers into their own feature.
    /// There is also an option to remove the chamfer from its original feature as well (removeChamfers bool).
    /// </summary>
    private void BreakOutChamfers()
    {
        List<Feature> featuresToAdd = new();
        foreach (Feature feature in features)
        {
            if (feature.ChamferList.Count <= 0) continue;

            /**
             * Manually set to true to remove chamfers from their base feature.
             * Doing this messes with the front end display so if you do
             * want to use this, that will need to be corrected.
             */
            bool removeChamfers = false;

            List<Line> chamfersToRemove = new();
            List<Entity> newEntityList = feature.EntityList.ToList();
            foreach (ChamferGroup cg in feature.ChamferList)
            {
                // Make new chamfer feature.
                featuresToAdd.Add(new Feature(PossibleFeatureTypes.Group3, [new Line(cg.Chamfer)]));

                // Add to a new list so that the indexes in entity list stay the same.
                chamfersToRemove.Add(cg.Chamfer);

                if (removeChamfers)
                {
                    EntityTools.ExtendTwoLines(newEntityList[cg.LineAIndex] as Line,
                        newEntityList[cg.LineBIndex] as Line);
                }
            }

            // Actually remove the chamfers.
            if (removeChamfers)
            {
                foreach (Line chamfer in chamfersToRemove)
                {
                    newEntityList.Remove(chamfer);
                }
            }

            feature.ChamferList.Clear();
            chamfersToRemove.Clear();
            feature.EntityList = newEntityList;
        }
        // Add the new chamfer features to the group.
        features.AddRange(featuresToAdd);
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

        // Break out chamfers.
        BreakOutChamfers();

        // Group identical features together.
        for (int i = 0; i < features.Count; i++)
        {
            // Ignore group 3 chamfer check because equals sees them as the same even though they are not.
            if (features[i].FeatureType == PossibleFeatureTypes.Group3)
            {
                continue;
            }

            for (int j = i + 1; j < features.Count; j++)
            {
                if (features[i].Equals(features[j]))
                {
                    features[i].count += features[j].count;
                    features.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    /// <summary>
    /// Check of all features in the group have a corresponding feature in other group, return true if they do.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not FeatureGroup fg) return false;

        if (fg == this) return true;

        // If here, obj is a FeatureGroup.
        if (!(_totalArcs == fg._totalArcs && _totalLines == fg._totalLines && _totalCircles == fg._totalCircles))
        {
            return false;
        }

        // Sort by perimeter.
        features.Sort((x, y) => x.perimeter.CompareTo(y.perimeter));
        fg.features.Sort((x, y) => x.perimeter.CompareTo(y.perimeter));

        for (int i = 0; i < features.Count; i++)
        {
            // While this features @ i has same perimeter as obj.features[j] check if any j = features[i].
            int j = i;
            bool checkPoint = false;
            while (j < features.Count &&
                   Math.Abs(features[i].perimeter - fg.features[j].perimeter) < Entity.EntityTolerance)
            {
                if (features[i].Equals(features[j]))
                {
                    checkPoint = true;
                    break;
                }

                // If first element checked isn't equal, increment j.
                j++;
            }

            if (!checkPoint) return false;
        }

        // If we got here, checkPoint was never false, so return true.
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