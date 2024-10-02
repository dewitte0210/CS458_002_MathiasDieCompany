using FeatureRecognitionAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace FeatureRecognitionAPI.Controllers
{
    [ApiController]
    [Route("FeatureRecognition")]
    public class FeatureRecognitionController
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IFeatureRecognitionService _featureRecognitionService;

        // TODO: Create API endpoint to take a .dwg, .dxf file and return info on its extension
        [HttpGet]
        [Route("getFileStructure")]
        public string GetFileStructure(string fileName)
        {
            //TODO
            string test = _featureRecognitionService.GetFileStructure(fileName);
            return test;
        }

        //[HttpPost]
        //[Route("uploadFile")]
        //public string UploadFile()
        //{
        //    var file = "";
        //    System.Web.
        //}
    }
}
