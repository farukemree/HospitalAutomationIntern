using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.DTOs
{
    public class UploadedFileDto
    {
        public IFormFile ImageFile { get; set; }
        public int UserId { get; set; }
    }
}
