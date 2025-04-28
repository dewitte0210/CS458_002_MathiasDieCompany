namespace FeatureRecognitionAPI.Models.Entities;

public class Punch(string cut, string @base, double value, int weight)
{
    public string Cut { get; set; } = cut;
    public string Base { get; set; } = @base;
    public double Value { get; set; } = value;
    public int Weight { get; set; } = weight;
}