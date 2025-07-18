using System.ComponentModel.DataAnnotations;

namespace HospitalAutomation.DataAccess.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        // Navigasyon propery ama Id eşitliğine dayalı olacak
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
    }

}
