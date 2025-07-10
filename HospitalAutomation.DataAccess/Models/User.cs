using System.ComponentModel.DataAnnotations;

namespace HospitalAutomation.DataAccess.Models
{
    public class User
    {
        [Key] // PRIMARY KEY olarak tanımlar
        public int Id { get; set; }  // <-- Bu olmazsa EF çalışmaz!

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
