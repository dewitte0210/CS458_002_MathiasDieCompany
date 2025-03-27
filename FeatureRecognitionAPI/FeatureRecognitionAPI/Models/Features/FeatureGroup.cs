using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Models.Features
{
    public class FeatureGroup
    {
        public int Count { get; set; } //Track how many feature groups of this type are found
        protected int totalArcs;
        protected int totalLines;
        protected int totalCircles;
        [JsonProperty] protected List<Feature> features;
        public List<List<Entity>> touchingEntities;

        public FeatureGroup(List<Feature> features)
        {
            // this.count = count;
            this.features = features;
            touchingEntities = new List<List<Entity>>();

            foreach (Feature feature in features)
            {
                this.totalArcs += feature.GetNumArcs();
                this.totalLines += feature.GetNumLines();
                this.totalCircles += feature.GetNumCircles();
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
    }
}