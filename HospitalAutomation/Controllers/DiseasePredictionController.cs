using HospitalAutomation.Service.Services;
using HospitalAutomation.Service.Interfaces;
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
            if (string.IsNullOrWhiteSpace(input.Symptoms))
                return BadRequest("Belirtiler boş olamaz.");

            var result = _onnxService.Predict(input.Symptoms);

            if (string.IsNullOrWhiteSpace(result))
                return StatusCode(500, "Modelden yanıt alınamadı.");

            return Ok(new { PredictedDisease = result });
        }
    }

    public class SymptomInputDto
    {
        public string Symptoms { get; set; }
    }
}
