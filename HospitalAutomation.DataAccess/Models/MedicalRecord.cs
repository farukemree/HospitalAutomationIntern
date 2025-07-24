using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public DateTime RecordDate { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
    }

}
