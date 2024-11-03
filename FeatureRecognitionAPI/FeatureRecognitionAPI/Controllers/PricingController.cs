using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

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

        [HttpPost("estimatePrice", Name = nameof(EstimatePrice))]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EstimatePrice([FromBody] string param) //TODO: create and replace as param object
        {
            //TODO: checks for correct input
            
            var (status, msg, output) = await _pricingService.EstimatePrice(param);

            if (status != OperationStatus.OK || output == null)
                return BadRequest(msg);

            return Ok(output);
        }
    }
}
