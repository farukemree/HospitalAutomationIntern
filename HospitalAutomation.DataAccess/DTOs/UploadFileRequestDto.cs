using Microsoft.AspNetCore.Http;

namespace HospitalAutomation.DataAccess.DTOs
{
    public class UploadFileRequestDto
    {
        public IFormFile? ImageFile { get; set; }
    }
}
