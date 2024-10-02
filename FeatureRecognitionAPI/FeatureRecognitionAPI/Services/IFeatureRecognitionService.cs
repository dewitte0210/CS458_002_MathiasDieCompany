using FeatureRecognitionAPI.Services;
using FeatureRecognitionAPI.Models;

namespace FeatureRecognitionAPI
{
    public interface IFeatureRecognitionService
    {
        public string GetFileStructure(string fileName);

    }
}
