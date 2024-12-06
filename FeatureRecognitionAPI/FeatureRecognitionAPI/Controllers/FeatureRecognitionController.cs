using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System.Web;

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

        // TODO: Create API endpoint to take a .dwg, .dxf file and return info on its extension
        [HttpGet("getFileExtension", Name = nameof(GetFileExtension))]
        public async Task<IActionResult> GetFileExtension([FromQuery] string fileName)
        {
            if (fileName == null)
                return BadRequest("File name cannot be null.");

            var (status, ext) = await _featureRecognitionService.GetFileExtension(fileName);

            if (status != OperationStatus.OK || ext == null)
                return BadRequest("Error detecting file extension.");

            return Ok(ext);
        }

        [HttpPost("uploadFile", Name = nameof(UploadFile))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null)
                return BadRequest("File cannot be null.");

            var (status, output) = await _featureRecognitionService.UploadFile(file);

            if (status != OperationStatus.OK || output == null)
                return BadRequest($"Error uploading file. OperationStatus: {status}, output: {output}");

            return Ok(output);
        }
    }
}
