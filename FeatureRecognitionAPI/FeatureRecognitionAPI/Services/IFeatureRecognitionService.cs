using FeatureRecognitionAPI.Models.Enums;

namespace FeatureRecognitionAPI.Services
{
    public interface IFeatureRecognitionService
    {
        public Task<string?> UploadFile(IFormFile file);
    }
}
