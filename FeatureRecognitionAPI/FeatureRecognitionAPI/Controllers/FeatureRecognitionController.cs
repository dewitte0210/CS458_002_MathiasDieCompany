using Microsoft.AspNetCore.Mvc;

namespace FeatureRecognitionAPI.Controllers
{
    [ApiController]
    [Route("FeatureRecognition")]
    public class FeatureRecognitionController
    {
        [HttpGet]
        public IActionResult GetFileStructure(string fileName)
        {
            //TODO
        }
    }
}
