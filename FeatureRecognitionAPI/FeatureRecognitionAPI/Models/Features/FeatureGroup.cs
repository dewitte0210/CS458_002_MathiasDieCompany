using FeatureRecognitionAPI.Services;

namespace FeatureRecognitionAPI.Models.Features
{
    public class FeatureGroup
    {
        protected int count; //Track how many feature groups of this type are found
        protected int totalArcs;
        protected int totalLines;
        protected int totalCircles;
        protected List<Feature> features;
        public FeatureGroup(int count, List<Feature> features) 
        {
            this.count = count;
            this.features = features;

            foreach( Feature feature in features)
            {
                this.totalArcs += feature.getNumArcs();
                this.totalLines += feature.getNumLines();
                this.totalCircles += feature.getNumCircles();
            }

        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
    }
}