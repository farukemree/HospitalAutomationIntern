using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }
        //  [Authorize(Roles = "Admin")]
        [HttpGet("GetAllMedicalRecords")]
        public IActionResult GetAllMedicalRecords()
        {
            var response = _medicalRecordService.GetAllMedicalRecords();
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        [HttpGet("GetMedicalRecordsByPatientId/{patientId}")]
        public IActionResult GetMedicalRecordsByPatientId(int patientId)
        {
            var result = _medicalRecordService.GetMedicalRecordsByPatientId(patientId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpGet("SearchMedicalRecords")]
        public async Task<IActionResult> SearchMedicalRecords([FromQuery] string keyword)
        {
            var result = _medicalRecordService.SearchMedicalRecordsByKeyword(keyword);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }




        //   [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("GetMedicalRecordById/{id}")]
        public IActionResult GetMedicalRecordById(int id)
        {
            var response = _medicalRecordService.GetMedicalRecordById(id);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }
        // [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("AddMedicalRecord")]
        public IActionResult AddMedicalRecord([FromBody] MedicalRecordDto medicalRecordDto)
        {
            if (medicalRecordDto == null)
                return BadRequest(ResponseBase.Error("Geçersiz tıbbi kayıt verisi."));

            var response = _medicalRecordService.AddMedicalRecord(medicalRecordDto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        // [Authorize(Roles = "Admin,Doctor")]
        [HttpPut("UpdateMedicalRecordById/{id}")]
        public IActionResult UpdateMedicalRecord(int id, [FromBody] MedicalRecordDto updatedMedicalRecord)
        {
            if (updatedMedicalRecord == null)
                return BadRequest(ResponseBase.Error("Güncelleme için tıbbi kayıt bilgisi gerekli."));

            var response = _medicalRecordService.UpdateMedicalRecord(id, updatedMedicalRecord);

            if (!response.IsSuccess)
                return response.Message.Contains("bulunamadı") ? NotFound(response) : BadRequest(response);

            return Ok(response);
        }
        // [Authorize(Roles = "Admin,Doctor")]
        [HttpDelete("DeleteMedicalRecordById/{id}")]
        public IActionResult DeleteMedicalRecord(int id)
        {
            var response = _medicalRecordService.DeleteMedicalRecordById(id);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }
    }
}
