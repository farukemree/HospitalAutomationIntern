using HospitalAutomation.DataAccess.Models;
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


namespace HospitalAutomation.Service.Interfaces
{
    public interface IPatientService
    {
        ResponseGeneric<List<PatientDto>> GetAllPatients();
        ResponseGeneric<PatientDto> GetPatientById(int id);
        ResponseBase AddPatient(Patient patient);
        ResponseBase UpdatePatient(int id, PatientDto updatedPatient);
        ResponseBase DeletePatientById(int id);
        string GetPatientNameById(int patientId);

    }
}
