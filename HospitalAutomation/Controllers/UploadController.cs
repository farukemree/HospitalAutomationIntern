using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }
       
        
        
        [HttpPost("UploadFile")]
        public async Task<IActionResult> 
            UploadFile([FromForm] UploadFileRequestDto request)
        {
            try
            {
                if (request.ImageFile == null || request.ImageFile.Length == 0)
                    return BadRequest("Resim dosyası boş olamaz.");

                
                string fileKey = Guid.NewGuid().ToString();

                var result = await _uploadService.UploadUserImageAsync(request.ImageFile, fileKey);

                if (!result.IsSuccess)
                    return BadRequest(result.Message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }




        [HttpGet("GetUserImage/{fileKey}")]
        public IActionResult GetUserImage(string fileKey)
        {
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Users");
            var files = Directory.GetFiles(uploadFolder);

            var matchedFile = files.FirstOrDefault(f =>
        Path.GetFileName(f).StartsWith(fileKey, StringComparison.OrdinalIgnoreCase));
            if (matchedFile == null)
                return NotFound("Resim bulunamadı.");

            var fileBytes = System.IO.File.ReadAllBytes(matchedFile);
            var contentType = GetContentType(matchedFile);
            return File(fileBytes, contentType); 
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".jfif" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }





    }
}
