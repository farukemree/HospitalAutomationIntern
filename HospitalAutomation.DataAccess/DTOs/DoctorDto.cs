using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.DTOs
{
    public class DoctorDto
    {
        public int Id { get; set; }            
        public string FullName { get; set; }   
        public string Specialization { get; set; } 
        public string Phone { get; set; }       
        public int DepartmentId { get; set; }
        public string? ImageFileKey { get; set; }
    }
}
