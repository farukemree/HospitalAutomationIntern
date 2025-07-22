using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    public class UserImage
    {
        public int Id { get; set; }          
        public string FileOriginalName { get; set; } = null!;
        public string FileGuidedName { get; set; } = null!;
        public string FileKey { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadDate { get; set; }
    }

}
