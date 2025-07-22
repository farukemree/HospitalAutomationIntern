using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string? FileKey { get; set; } = null!;
        public string Base64Data { get; set; } = null!;
    }
}
