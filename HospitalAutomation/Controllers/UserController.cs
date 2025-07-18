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

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var result = _userService.GetAllUsers();

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        //[Authorize(Roles = "Admin")]
        [HttpPost("UpdateUserRole")]
        public IActionResult UpdateRole([FromBody] UpdateUserRoleDto dto)
        {
            if (dto == null || dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.NewRole))
                return BadRequest("Geçersiz parametreler.");

            var response = _userService.UpdateUserRole(dto);

            if (!response.IsSuccess)
                return BadRequest(response.Message);

            return Ok(response);
        }





        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterRequestDto request)
        {
            if (request == null || request.User == null)
                return BadRequest("Geçersiz kayıt verisi.");

            var response = _userService.Register(request.User, request.Patient);

            if (!response.IsSuccess)
                return BadRequest(response.Message);

            return Ok(response);
        }







        // [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminArea()
        {
            return Ok("Bu alan sadece admin kullanıcılar içindir.");
        }


        // [Authorize(Roles = "Doctor")]
        [HttpGet("doctor-area")]
        public IActionResult DoctorArea()
        {
            return Ok("Bu alan sadece doktorlar içindir.");
        }


        //[Authorize(Roles = "Patient")]
        [HttpGet("patient-area")]
        public IActionResult PatientArea()
        {
            return Ok("Bu alan sadece hastalar içindir.");
        }
    }
}
