using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // [Authorize(Roles = "Admin")]
        [HttpGet("GetAllDepartments")]
        public IActionResult GetAllDepartments()
        {
            var response = _departmentService.GetAllDepartments();
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }

        // [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("GetDepartmentById/{id:int}")]
        public IActionResult GetDepartmentById(int id)
        {
            var response = _departmentService.GetDepartmentById(id);

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }


        //[Authorize(Roles = "Admin")]
        [HttpPost("AddDepartment")]
        public IActionResult AddDepartment([FromBody] DepartmentDto dto)
        {
            if (dto == null)
                return BadRequest(ResponseBase.Error("Geçersiz departman verisi."));

            var result = _departmentService.AddDepartment(dto);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        //  [Authorize(Roles = "Admin")]
        [HttpPut("UpdateDepartmentById/{id:int}")]
        public IActionResult UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            if (dto == null)
                return BadRequest(ResponseBase.Error("Güncelleme için departman bilgisi gerekli."));

            var result = _departmentService.UpdateDepartment(id, dto);
            if (!result.IsSuccess && result.Message.Contains("bulunamadı"))
                return NotFound(result);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("DeleteDepartmentById/{id:int}")]
        public IActionResult DeleteDepartment(int id)
        {
            var result = _departmentService.DeleteDepartmentById(id);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
