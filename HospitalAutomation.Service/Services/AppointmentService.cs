using HospitalAutomation.DataAccess.Context;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ResponseBase = HospitalAutomation.Service.Response.Response;

namespace HospitalAutomation.Service.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(AppDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ResponseGeneric<List<AppointmentDto>> GetAllAppointments()
        {
            try
            {
                var sql = "EXEC Pr_GetAllAppointments";
                var appointments = _context.Appointments.FromSqlRaw(sql).ToList();

                if (!appointments.Any())
                {
                    _logger.LogWarning("Veritabanında hiç randevu bulunamadı.");
                    return ResponseGeneric<List<AppointmentDto>>.Error("Randevu bulunamadı.");
                }

                var dtoList = appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    Description = a.Description
                }).ToList();

                _logger.LogInformation("Tüm randevular başarıyla getirildi.");
                return ResponseGeneric<List<AppointmentDto>>.Success(dtoList, "Randevular başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevular getirilirken hata oluştu.");
                return ResponseGeneric<List<AppointmentDto>>.Error("Veriler alınırken hata oluştu.");
            }
        }

        public ResponseGeneric<List<AppointmentDto>> GetAppointmentsByPatientId(int patientId)
        {
            try
            {
                var sql = "EXEC Pr_GetAppointmentsByPatientId @PatientId";
                var param = new SqlParameter("@PatientId", patientId);

                var appointments = _context.Appointments
                    .FromSqlRaw(sql, param)
                    .ToList();

                if (!appointments.Any())
                {
                    _logger.LogWarning($"PatientId {patientId} için randevu bulunamadı.");
                    return ResponseGeneric<List<AppointmentDto>>.Error("Randevu bulunamadı.");
                }

                var dtoList = appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    Description = a.Description
                }).ToList();

                _logger.LogInformation($"PatientId {patientId} için randevular başarıyla getirildi.");
                return ResponseGeneric<List<AppointmentDto>>.Success(dtoList, "Randevular başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasta randevuları getirilirken hata oluştu.");
                return ResponseGeneric<List<AppointmentDto>>.Error("Veriler alınırken hata oluştu.");
            }
        }


        public ResponseGeneric<AppointmentDto> GetAppointmentById(int id)
        {
            try
            {
                var sql = "EXEC Pr_GetAppointmentById @Id";
                var param = new SqlParameter("@Id", id);

                var appointment = _context.Appointments
                    .FromSqlRaw(sql, param)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (appointment == null)
                {
                    _logger.LogWarning("Randevu bulunamadı. ID: {Id}", id);
                    return ResponseGeneric<AppointmentDto>.Error("Randevu bulunamadı.");
                }

                _logger.LogInformation("Randevu getirildi. ID: {Id}", id);

                var dto = new AppointmentDto
                {
                    Id = appointment.Id,  
                    AppointmentDate = appointment.AppointmentDate,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    Description = appointment.Description
                };
                return ResponseGeneric<AppointmentDto>.Success(dto, "Randevu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu getirilirken hata oluştu. ID: {Id}", id);
                return ResponseGeneric<AppointmentDto>.Error("Randevu getirilirken hata oluştu.");
            }
        }

        public ResponseGeneric<AppointmentDto> AddAppointment(AppointmentDto dto)
        {
            try
            {
                var sql = "EXEC Pr_Add_Appointment @AppointmentDate, @PatientId, @DoctorId, @Description";
                var parameters = new[]
                {
                    new SqlParameter("@AppointmentDate", dto.AppointmentDate),
                    new SqlParameter("@PatientId", dto.PatientId),
                    new SqlParameter("@DoctorId", dto.DoctorId),
                    new SqlParameter("@Description", dto.Description ?? (object)DBNull.Value)
                };

                _context.Database.ExecuteSqlRaw(sql, parameters);
                _logger.LogInformation("Randevu başarıyla eklendi.");
                return ResponseGeneric<AppointmentDto>.Success(dto, "Randevu başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu eklenirken hata oluştu.");
                return ResponseGeneric<AppointmentDto>.Error("Randevu eklenirken hata oluştu: " + ex.Message);
            }
        }

        public ResponseGeneric<string> UpdateAppointment(int id, AppointmentDto dto)
        {
            try
            {
                var sql = "EXEC Pr_Update_Appointment @Id, @AppointmentDate, @PatientId, @DoctorId, @Description";
                var parameters = new[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@AppointmentDate", dto.AppointmentDate),
            new SqlParameter("@PatientId", dto.PatientId),
            new SqlParameter("@DoctorId", dto.DoctorId),
            new SqlParameter("@Description", dto.Description ?? (object)DBNull.Value)
        };

                int rows = _context.Database.ExecuteSqlRaw(sql, parameters);
                if (rows == 0)
                    return ResponseGeneric<string>.Error("Randevu bulunamadı.");

                _logger.LogInformation("Randevu güncellendi. ID: {Id}", id);
                return ResponseGeneric<string>.Success("Randevu güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu güncellenirken hata oluştu.");
                return ResponseGeneric<string>.Error($"Hata: {ex.Message}");
            }
        }


        public ResponseBase DeleteAppointmentById(int id)
        {
            try
            {
                var sql = "EXEC Pr_Delete_Appointment @Id";
                var param = new SqlParameter("@Id", id);

                int rows = _context.Database.ExecuteSqlRaw(sql, param);
                if (rows == 0)
                {
                    _logger.LogWarning("Silinecek randevu bulunamadı. ID: {Id}", id);
                    return ResponseBase.Error("Randevu bulunamadı.");
                }

                _logger.LogInformation("Randevu silindi. ID: {Id}", id);
                return ResponseBase.Success("Randevu başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu silinirken hata oluştu.");
                return ResponseBase.Error($"Hata: {ex.Message}");
            }
        }


    }
}
