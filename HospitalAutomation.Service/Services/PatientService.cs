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
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PatientService> _logger;

        public PatientService(AppDbContext context, ILogger<PatientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ResponseGeneric<List<PatientDto>> GetAllPatients()
        {
            try
            {
                var sql = "EXEC Pr_GetAllPatients";
                var patients = _context.Patients.FromSqlRaw(sql).ToList();

                if (!patients.Any())
                {
                    _logger.LogWarning("Veritabanında hiç hasta bulunamadı.");
                    return ResponseGeneric<List<PatientDto>>.Error("Hasta bulunamadı.");
                }

                var dtoList = patients.Select(p => new PatientDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender
                }).ToList();

                _logger.LogInformation("Tüm hastalar başarıyla getirildi. Toplam kayıt: {Count}", dtoList.Count);
                return ResponseGeneric<List<PatientDto>>.Success(dtoList, "Hastalar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hastalar getirilirken hata oluştu.");
                return ResponseGeneric<List<PatientDto>>.Error("Hastalar getirilirken hata oluştu.");
            }
        }

        public ResponseGeneric<PatientDto> GetPatientById(int id)
        {
            try
            {
                var sql = "EXEC Pr_GetPatientById @Id";
                var param = new SqlParameter("@Id", id);

                var patient = _context.Patients
                    .FromSqlRaw(sql, param)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (patient == null)
                {
                    _logger.LogWarning("Hasta bulunamadı. ID: {Id}", id);
                    return ResponseGeneric<PatientDto>.Error("Hasta bulunamadı.");
                }

                var dto = new PatientDto
                {
                    Id = patient.Id,
                    FullName = patient.FullName,
                    BirthDate = patient.BirthDate,
                    Gender = patient.Gender
                };

                _logger.LogInformation("Hasta başarıyla getirildi. ID: {Id}", id);
                return ResponseGeneric<PatientDto>.Success(dto, "Hasta başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasta getirilirken hata oluştu. ID: {Id}", id);
                return ResponseGeneric<PatientDto>.Error("Hasta getirilirken hata oluştu.");
            }
        }

        public ResponseBase AddPatient(Patient patient)
        {
            try
            {
                var existingPatient = _context.Patients
                    .FromSqlRaw("SELECT * FROM Patients WHERE FullName = {0}", patient.FullName)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (existingPatient != null)
                    return ResponseBase.Error("Bu isimde hasta zaten mevcut.");

                var sql = "EXEC Pr_Add_Patient @FullName, @BirthDate, @Gender, @AppointmentDate";
                var parameters = new[]
                {
                    new SqlParameter("@FullName", patient.FullName),
                    new SqlParameter("@BirthDate", patient.BirthDate),
                    new SqlParameter("@Gender", patient.Gender ?? (object)DBNull.Value),
                    new SqlParameter("@AppointmentDate", patient.AppointmentDate ?? (object)DBNull.Value)
                };

                _context.Database.ExecuteSqlRaw(sql, parameters);

                _logger.LogInformation("Hasta başarıyla eklendi: {FullName}", patient.FullName);
                return ResponseBase.Success("Hasta başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasta eklenirken hata oluştu: {FullName}", patient.FullName);
                return ResponseBase.Error($"Hasta eklenirken hata oluştu: {ex.Message}");
            }
        }

        public ResponseBase UpdatePatient(int id, PatientDto updatedPatientDto)
        {
            try
            {
                var sql = "EXEC Pr_Update_Patient @Id, @FullName, @BirthDate, @Gender";
                var parameters = new[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@FullName", updatedPatientDto.FullName ?? (object)DBNull.Value),
                    new SqlParameter("@BirthDate", updatedPatientDto.BirthDate),
                    new SqlParameter("@Gender", updatedPatientDto.Gender ?? (object)DBNull.Value)
                };

                int affectedRows = _context.Database.ExecuteSqlRaw(sql, parameters);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Güncellenmek istenen hasta bulunamadı. ID: {Id}", id);
                    return ResponseBase.Error("Hasta bulunamadı.");
                }

                _logger.LogInformation("Hasta başarıyla güncellendi. ID: {Id}, Ad: {Name}", id, updatedPatientDto.FullName);
                return ResponseBase.Success("Hasta başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasta güncellenirken hata oluştu. ID: {Id}", id);
                return ResponseBase.Error($"Hasta güncellenirken hata oluştu: {ex.Message}");
            }
        }

        public ResponseBase DeletePatientById(int id)
        {
            try
            {
                var sql = "EXEC Pr_Delete_Patient @Id";
                var param = new SqlParameter("@Id", id);

                int affectedRows = _context.Database.ExecuteSqlRaw(sql, param);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Silinmek istenen hasta bulunamadı. ID: {Id}", id);
                    return ResponseBase.Error("Hasta bulunamadı.");
                }

                _logger.LogInformation("Hasta başarıyla silindi. ID: {Id}", id);
                return ResponseBase.Success("Hasta başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasta silinirken hata oluştu. ID: {Id}", id);
                return ResponseBase.Error($"Hasta silinirken hata oluştu: {ex.Message}");
            }
        }
    }
}
