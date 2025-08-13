using Azure.Core;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiseasePredictionController : ControllerBase
    {
        private readonly IOnnxService _onnxService;

        public DiseasePredictionController(IOnnxService onnxService)
        {
            _onnxService = onnxService;
        }

        [HttpPost("Predict")]
        public IActionResult Predict([FromBody] SymptomInputDto input)
        {
            try
            {
                var result = _onnxService.Predict(input.Symptoms);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class SymptomInputDto
        {
            public string Symptoms { get; set; }
        }
    }
}
