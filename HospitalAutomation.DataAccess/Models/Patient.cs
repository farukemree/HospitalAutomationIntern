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
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public string Gender { get; set; }

        public DateTime? AppointmentDate { get; set; }

        // Bu satır önemli değil, sadece çift yönlü ilişki istiyorsan eklenebilir
        public virtual User User { get; set; }
    }



}
