using FeatureRecognitionAPI.Services;
using FeatureRecognitionAPI.Models;

namespace FeatureRecognitionAPI
{
    public interface IFeatureRecognitionService
    {
        public SupportedFile GetFileStructure(string fileName);

    }
}
