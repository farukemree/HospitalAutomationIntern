using Microsoft.AspNetCore.Mvc;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IResetPasswordService _passwordResetService;

        public PasswordResetController(IResetPasswordService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var response = await _passwordResetService.SendResetCodeAsync(dto);  // dto olarak gönder
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyResetCodeDto dto)
        {
            var response = await _passwordResetService.VerifyResetCodeAsync(dto);  // dto olarak gönder
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var response = await _passwordResetService.ResetPasswordAsync(dto);  // dto olarak gönder
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

    }
}
