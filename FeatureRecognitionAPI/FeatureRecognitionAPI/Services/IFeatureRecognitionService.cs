using FeatureRecognitionAPI.Services;
using FeatureRecognitionAPI.Models;
using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Services
{
    public interface IFeatureRecognitionService
    {
        public Task<(OperationStatus, string)> GetFileExtension(string fileName);
        public Task<(OperationStatus, int)> UploadFile(IFormFile file);

    }
}
