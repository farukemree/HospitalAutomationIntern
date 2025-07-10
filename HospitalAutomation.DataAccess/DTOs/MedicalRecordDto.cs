using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace HospitalAutomation.DataAccess.DTOs
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }               
        public int PatientId { get; set; }        
        public DateTime RecordDate { get; set; }
        public string Description { get; set; }  
    }
}

