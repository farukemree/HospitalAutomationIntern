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
    public interface IDepartmentService
    {
        ResponseGeneric<List<DepartmentDto>> GetAllDepartments();
        ResponseGeneric<DepartmentDto> GetDepartmentById(int id);
        ResponseBase AddDepartment(DepartmentDto departmentDto);
        ResponseBase UpdateDepartment(int id, DepartmentDto updatedDepartment);
        ResponseBase DeleteDepartmentById(int id);
    }
}

