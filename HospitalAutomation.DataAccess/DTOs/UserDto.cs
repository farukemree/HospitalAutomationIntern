using HospitalAutomation.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.DTOs
{
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
    public class UpdateUserRoleDto
    {
        public int UserId { get; set; }
        public string NewRole { get; set; }
    }
    public class RegisterRequestDto
    {
        public UserRegisterDto User { get; set; }
        public PatientDto Patient { get; set; }


    }
}
