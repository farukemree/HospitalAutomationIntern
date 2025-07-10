using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim zorunludur")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur")]
        public string? Gender { get; set; }
        public DateTime? AppointmentDate { get; set; }
    }


}
