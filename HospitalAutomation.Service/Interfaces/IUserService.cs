using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.Service.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Interfaces
{
   public interface IUserService
    {
        public ResponseGeneric<string> Login(UserLoginDto user);
        public ResponseGeneric<bool> Register(UserRegisterDto userDto, PatientDto patientDto = null);
        public ResponseGeneric<bool> UpdateUserRole(UpdateUserRoleDto dto);
        ResponseGeneric<List<UserDto>> GetAllUsers();

    }
}
