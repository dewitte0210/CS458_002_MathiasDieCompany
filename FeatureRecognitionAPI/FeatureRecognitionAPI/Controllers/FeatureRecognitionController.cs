using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FeatureRecognitionAPI.Controllers
{
    [ApiController]
    [Route("FeatureRecognition")]
    public class FeatureRecognitionController
    {
        [HttpGet]
        [Route("getFileStructure")]
        public string GetFileStructure(string fileName)
        {
            return null;
            //TODO
            //FeatureRecognitionService.GetFileStructure(fileName);
            return "test";
        }
    }
}
