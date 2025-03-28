using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Features
{
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
                this.totalArcs += feature.getNumArcs();
                this.totalLines += feature.getNumLines();
                this.totalCircles += feature.getNumCircles();
            }
        }

        public void setFeatureList(List<Feature> features)
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
}