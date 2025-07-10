using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllPatients")]
        public IActionResult GetAllPatients()
        {
            var response = _patientService.GetAllPatients();

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("GetPatientById/{id}")]
        public IActionResult GetPatientById(int id)
        {
            var response = _patientService.GetPatientById(id);

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
        [AllowAnonymous]
        [HttpPost("AddPatient")]
        public IActionResult AddPatient([FromBody] Patient patient)
        {
            if (patient == null)
                return BadRequest(ResponseBase.Error("Geçersiz hasta verisi."));

            var response = _patientService.AddPatient(patient);

            if (!response.IsSuccess)
                return BadRequest(response);

            // CreatedAtAction için DTO kullanabilirsin, burada örnek amaçlı direkt entity kullandım
            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, response);
        }
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPut("UpdatePatientById/{id}")]
        public IActionResult UpdatePatient(int id, [FromBody] PatientDto updatedPatient)
        {
            if (updatedPatient == null)
                return BadRequest(ResponseBase.Error("Güncelleme için hasta bilgisi gerekli."));

            var response = _patientService.UpdatePatient(id, updatedPatient);

            if (!response.IsSuccess && response.Message.Contains("bulunamadı"))
                return NotFound(response);

            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
        [Authorize(Roles = "Admin,Doctor")]
        [HttpDelete("DeletePatientById/{id}")]
        public IActionResult DeletePatient(int id)
        {
            var response = _patientService.DeletePatientById(id);

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
    }
}
