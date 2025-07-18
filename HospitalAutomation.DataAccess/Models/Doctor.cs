using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    public class Doctor
    {
        [Key, ForeignKey("User")]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public virtual User User { get; set; }
    }

}
