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
    public interface IMedicalRecordService
    {
        ResponseGeneric<List<MedicalRecordDto>> GetAllMedicalRecords();
        ResponseGeneric<MedicalRecordDto> GetMedicalRecordById(int id);
        ResponseBase AddMedicalRecord(MedicalRecordDto medicalRecordDto);
        ResponseBase UpdateMedicalRecord(int id, MedicalRecordDto updatedMedicalRecord);
        ResponseBase DeleteMedicalRecordById(int id);
        public ResponseGeneric<List<MedicalRecordDto>> GetMedicalRecordsByPatientId(int patientId);
    }
}
