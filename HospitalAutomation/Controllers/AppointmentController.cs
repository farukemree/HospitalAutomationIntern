using HospitalAutomation.API.Hubs;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IPatientService _patientService;   

        public AppointmentController(IAppointmentService appointmentService, IHubContext<NotificationHub> hubContext, IPatientService patientService)
        {
            _appointmentService = appointmentService;
            _hubContext = hubContext;
            _patientService = patientService;
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
        [HttpGet("GetAppointmentsByDoctorId/{doctorId}")]
        public IActionResult GetAppointmentsByDoctorId(int doctorId)
        {
            var response = _appointmentService.GetAppointmentsByDoctorId(doctorId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
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

            // Eğer randevu yoksa hata döndürme, başarı olarak dön ama data boş liste olsun
            if (result.IsSuccess == false && result.Data == null)
            {
                // Randevu yok demek, bunu hata değil normal durum olarak kabul et
                return Ok(new
                {
                    Data = new List<AppointmentDto>(),
                    IsSuccess = true,
                    Message = "Henüz randevunuz yok."
                });
            }

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

            // PatientId üzerinden hasta adını al
            var patientName = _patientService.GetPatientNameById(appointmentDto.PatientId);

            // 📌 Sadece ilgili doktora bildirim gönder
            _hubContext.Clients
                .Group(appointmentDto.DoctorId.ToString()) // DoctorId'ye özel grup
                .SendAsync("ReceiveMessage", "Sistem", $"{patientName} adlı hasta size randevu aldı!");

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
