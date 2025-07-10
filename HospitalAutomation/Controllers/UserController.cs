using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

       
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLoginDto user)
        {
            if (user == null)
                return BadRequest("Geçersiz kullanıcı verisi.");

            var response = _userService.Login(user);

            if (!response.IsSuccess)
                return Unauthorized(response.Message);

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("UpdateUserRole")]
        public IActionResult UpdateRole([FromQuery] string username, [FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
                return BadRequest("Kullanıcı adı ve rol boş olamaz.");

            var response = _userService.UpdateUserRole(username, role);

            if (!response.IsSuccess)
                return BadRequest(response.Message);

            return Ok(response);
        }





        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserRegisterDto user)
        {
            if (user == null)
                return BadRequest("Geçersiz kullanıcı verisi.");

            var response = _userService.Register(user);

            if (!response.IsSuccess)
                return BadRequest(response.Message);

            return Ok(response);
        }

       
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminArea()
        {
            return Ok("Bu alan sadece admin kullanıcılar içindir.");
        }

      
        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor-area")]
        public IActionResult DoctorArea()
        {
            return Ok("Bu alan sadece doktorlar içindir.");
        }

      
        [Authorize(Roles = "Patient")]
        [HttpGet("patient-area")]
        public IActionResult PatientArea()
        {
            return Ok("Bu alan sadece hastalar içindir.");
        }
    }
}
