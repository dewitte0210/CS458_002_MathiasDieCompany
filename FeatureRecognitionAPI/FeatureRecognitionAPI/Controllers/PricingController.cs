using FeatureRecognitionAPI.Models.Dtos;
using FeatureRecognitionAPI.Models.Enums;
using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using System.Web.Http;
using System.Web.Http.Results;
using FeatureRecognitionAPI.Models.Pricing;
using FromBody = System.Web.Http.FromBodyAttribute;

namespace FeatureRecognitionAPI.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/Pricing")]
    [ApiController]
    public class PricingController(IPricingService pricingService, IPricingDataService dataService)
        : ControllerBase
    {
        [Microsoft.AspNetCore.Mvc.HttpPost("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EstimatePrice([Microsoft.AspNetCore.Mvc.FromBody] JsonElement param)
        {
            try
            {
                var text = param.GetRawText();
                var quoteSubmissionDto = JsonConvert.DeserializeObject<QuoteSubmissionDto>(param.GetRawText());
                if (quoteSubmissionDto == null)
                    return BadRequest("Invalid payload.");

                var (status, msg, output) = pricingService.EstimatePrice(quoteSubmissionDto);

                if (status != OperationStatus.OK || output == null)
                    return BadRequest(msg);

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid JSON format: {ex.Message}");
            }
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<FeaturePrice>>> GetFeaturePrices()
        {
            return Ok(dataService.GetFeaturePrices());
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("[action]")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PunchPriceReturn>> GetPunchPrices()
        {
            return Ok(dataService.GetPunchPrices());
        }

        [Microsoft.AspNetCore.Mvc.HttpPost("[action]/{type}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdatePunchPrice(PossibleFeatureTypes type, [FromBody] List<PunchPrice> punches)
        {
            // Punch is included here because we do not have prices for generic punches
            // type > 7 is if a feature price was sent to the punch method
            if (type is PossibleFeatureTypes.Unknown or PossibleFeatureTypes.Punch
                || (int)type > 7)
            {
                return BadRequest();
            }

            bool success = dataService.UpdatePunchPrice(type, punches);
            return success ? Ok() : StatusCode(500);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost("[action]/{type}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateFeaturePrice([FromBody] List<FeaturePrice> features)
        {
            // type <= 7 is if a punch price was sent to the feature method
            bool success = dataService.UpdateFeaturePrice(features);
            return success ? Ok() : StatusCode(500);
        }

        [Microsoft.AspNetCore.Mvc.HttpPost("[action]/{type}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeletePunchPrice(PossibleFeatureTypes type, [FromBody] PunchPrice price)
        {
            if (type is PossibleFeatureTypes.Unknown or PossibleFeatureTypes.Punch
                || (int)type >= 7) { return BadRequest(); }
            bool success = dataService.deletePunchPrice(type, price);
            return success ? Ok() : StatusCode(500);
        }
        
        [Microsoft.AspNetCore.Mvc.HttpPost("[action]/{type}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddPunchPrice(PossibleFeatureTypes type, [FromBody] PunchPrice price)
        {
            if (type is PossibleFeatureTypes.Unknown or PossibleFeatureTypes.Punch
                || (int)type >= 7) { return BadRequest(); }
            bool success = dataService.AddPunchPrice(type, price);
            return success ? Ok() : StatusCode(500);
        }
    }
}
