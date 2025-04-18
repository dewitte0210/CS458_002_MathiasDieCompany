using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FeatureRecognitionAPI.Controllers
{
    [Route("api/FeatureRecognition")]
    [ApiController]
    public class FeatureRecognitionController : ControllerBase
    {
        private readonly IFeatureRecognitionService _featureRecognitionService;

        public FeatureRecognitionController(IFeatureRecognitionService featureRecognitionService)
        {
            _featureRecognitionService = featureRecognitionService;
        }

        [HttpPost("uploadFile", Name = nameof(UploadFile))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null)
                return BadRequest("File cannot be null.");

            try
            {
                return Ok(await _featureRecognitionService.UploadFile(file));
            }
            catch(Exception e)
            {
                //Send error message as JSON so we can access it easier in the front end using await .json()
                return BadRequest(JsonConvert.SerializeObject($"Error uploading file. {e.Message}"));
            }
        }
    }
}
