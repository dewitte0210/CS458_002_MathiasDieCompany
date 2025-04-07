using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using FeatureRecognitionAPI.Models.Pricing;

namespace FeatureRecognitionAPI.Controllers
{
    [Route("api/Pricing")]
    [ApiController]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;

        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EstimatePrice([FromBody] JsonElement param)
        {
            try
            {
                var text = param.GetRawText();
                var quoteSubmissionDto = JsonConvert.DeserializeObject<QuoteSubmissionDto>(param.GetRawText());
                if (quoteSubmissionDto == null)
                    return BadRequest("Invalid payload.");

                var (status, msg, output) =  _pricingService.EstimatePrice(quoteSubmissionDto);

                if (status != OperationStatus.OK || output == null)
                    return BadRequest(msg);

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid JSON format: {ex.Message}");
            }
        }
        
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)] 
        public async Task<List<FeaturePrice>> GetFeaturePrices()
        {
            return _pricingService.GetFeaturePrices();
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<PunchPriceReturn> GetPunchPrices()
        {
            return _pricingService.GetPunchPrices();
        }
    }
}
