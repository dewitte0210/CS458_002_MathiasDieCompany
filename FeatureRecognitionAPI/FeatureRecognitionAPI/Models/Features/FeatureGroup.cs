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

            return features.Count == objF.features.Count && features.All(objF.features.Contains) && objF.features.All(features.Contains);
        }
    }
}