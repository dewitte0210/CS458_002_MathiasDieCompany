using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FeatureRecognitionAPI.Controllers
{
    [ApiController]
    [Route("FeatureRecognition")]
    public class FeatureRecognitionController
    {
        // TODO: Create API endpoint to take a .dwg, .dxf file and return info on its extension
        [HttpGet]
        [Route("getFileStructure")]
        public string GetFileStructure(string fileName)
        {
            //TODO
            string test = FeatureRecognitionService.GetFileStructure(fileName);
            return test;
        }
    }
}
