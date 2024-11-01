namespace FeatureRecognitionAPI.Models.Features
{
    public class FeatureGroup
    {
        protected int count;
        protected List<Feature> features;
        public FeatureGroup(int count, List<Feature> features) 
        {
            this.count = count;
            this.features = features;
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
    }
}