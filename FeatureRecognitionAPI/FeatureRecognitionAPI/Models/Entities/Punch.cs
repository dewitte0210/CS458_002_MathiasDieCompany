namespace FeatureRecognitionAPI.Models.Entities
{
    public class Punch
    {
        public string Cut { get; set; }
        public string Base { get; set; }
        public double Value { get; set; }
        public int Weight { get; set; }

        public Punch(string cut, string @base, double value, int weight ) {
            Cut = cut;
            Base = @base;
            Value = value;
            Weight = weight;
        }

    }
}
