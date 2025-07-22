using HospitalAutomation.DataAccess.Context;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly AppDbContext _context;

        public DoctorController(IDoctorService doctorService, AppDbContext context)
        {
            _doctorService = doctorService;
            _context = context;
        }
        //  [Authorize(Roles = "Admin")]
        [HttpGet("GetAllDoctors")]
        public IActionResult GetAllDoctors()
        {
            var response = _doctorService.GetAllDoctors();
            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
        //  [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("GetDoctorById/{id:int}")]
        public IActionResult GetDoctorById(int id)
        {
            var response = _doctorService.GetDoctorById(id);
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
        // [Authorize(Roles = "Admin")]
        [HttpPost("AddDoctor")]
        public IActionResult AddDoctor([FromBody] DoctorDto doctorDto)
        {
            if (doctorDto == null)
                return BadRequest(ResponseBase.Error("Doktor verisi boş olamaz."));

            var response = _doctorService.AddDoctor(doctorDto);
            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }


        //  [Authorize(Roles = "Admin")]
        [HttpPut("UpdateDoctorById/{id:int}")]
        public IActionResult UpdateDoctor(int id, [FromBody] DoctorDto updatedDoctor)
        {
            if (updatedDoctor == null)
                return BadRequest(ResponseBase.Error("Güncelleme verisi eksik."));

            var response = _doctorService.UpdateDoctor(id, updatedDoctor);
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
        // [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteDoctorById/{id:int}")]
        public IActionResult DeleteDoctor(int id)
        {
            var response = _doctorService.DeleteDoctorById(id);
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
    }
}
