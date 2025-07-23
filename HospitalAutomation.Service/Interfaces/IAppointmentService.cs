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
    public interface IAppointmentService
    {
        ResponseGeneric<List<AppointmentDto>> GetAllAppointments();
        ResponseGeneric<AppointmentDto> GetAppointmentById(int id);
        public ResponseGeneric<AppointmentDto> AddAppointment(AppointmentDto appointmentDto);
        public ResponseGeneric<string> UpdateAppointment(int id, AppointmentDto updatedAppointment);
        public ResponseBase DeleteAppointmentById(int id);
        public ResponseGeneric<List<AppointmentDto>> GetAppointmentsByPatientId(int patientId);

        ResponseGeneric<List<AppointmentDto>> GetAppointmentsByDoctorId(int doctorId);


    }
}
