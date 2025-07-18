using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        //[Authorize(Roles = "Admin,Doctor")]
        [HttpGet("GetAllAppointments")]
        public IActionResult GetAllAppointments()
        {
            var response = _appointmentService.GetAllAppointments();

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("GetAppointmentById/{id}")]
        public IActionResult GetAppointmentById(int id)
        {
            var response = _appointmentService.GetAppointmentById(id);

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response); 
        }
       
        [HttpGet("GetAppointmentsByPatientId/{patientId}")]
        public IActionResult GetAppointmentsByPatientId(int patientId)
        {
            var result = _appointmentService.GetAppointmentsByPatientId(patientId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }



        //[Authorize(Roles = "Patient,Doctor")]
        [HttpPost("AddAppointment")]
        public IActionResult AddAppointment([FromBody] AppointmentDto appointmentDto)
        {
            if (appointmentDto == null)
                return BadRequest(ResponseBase.Error("Geçersiz randevu verisi."));

            var result = _appointmentService.AddAppointment(appointmentDto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result); 
        }


        //[Authorize(Roles = "Admin,Doctor,Patient")]
        [HttpPut("UpdateAppointmentById/{id}")]
        public IActionResult UpdateAppointment(int id, [FromBody] AppointmentDto updatedAppointment)
        {
            if (updatedAppointment == null)
                return BadRequest(ResponseBase.Error("Güncelleme için randevu bilgisi gerekli."));

            var result = _appointmentService.UpdateAppointment(id, updatedAppointment);

            if (!result.IsSuccess && result.Message.Contains("bulunamadı"))
                return NotFound(result);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result); 
        }

        //[AllowAnonymous]
        [HttpDelete("DeleteAppointmentById/{id}")]
        public IActionResult DeleteAppointment(int id)
        {
            var result = _appointmentService.DeleteAppointmentById(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

    }
}
