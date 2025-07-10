using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Response;
using System.Collections.Generic;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Response;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResponseBase = HospitalAutomation.Service.Response.Response;

using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Response;
using System.Collections.Generic;
namespace HospitalAutomation.Service.Interfaces
{
    public interface IDoctorService
    {
        ResponseGeneric<List<DoctorDto>> GetAllDoctors();
        ResponseGeneric<DoctorDto> GetDoctorById(int id);
        ResponseGeneric<DoctorDto> AddDoctor(DoctorDto doctorDto);
        ResponseGeneric<string> UpdateDoctor(int id, DoctorDto updatedDoctor);
        ResponseBase DeleteDoctorById(int id);
    }
}
