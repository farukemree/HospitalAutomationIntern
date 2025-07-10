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
    public class DoctorService : IDoctorService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(AppDbContext context, ILogger<DoctorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ResponseGeneric<List<DoctorDto>> GetAllDoctors()
        {
            try
            {
                var sql = "EXEC Pr_GetAllDoctors";
                var doctors = _context.Doctors.FromSqlRaw(sql).ToList();

                if (!doctors.Any())
                {
                    _logger.LogWarning("Veritabanında hiç doktor bulunamadı.");
                    return ResponseGeneric<List<DoctorDto>>.Error("Doktor bulunamadı.");
                }

                var doctorDtos = doctors.Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Specialization = d.Specialization,
                    Phone = d.Phone,
                    DepartmentId = d.DepartmentId
                }).ToList();

                _logger.LogInformation("Tüm doktorlar başarıyla getirildi.");
                return ResponseGeneric<List<DoctorDto>>.Success(doctorDtos, "Doktorlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doktorlar getirilirken hata oluştu.");
                return ResponseGeneric<List<DoctorDto>>.Error("Veriler alınırken hata oluştu.");
            }
        }

        public ResponseGeneric<DoctorDto> GetDoctorById(int id)
        {
            try
            {
                var sql = "EXEC Pr_GetDoctorById @Id";
                var param = new SqlParameter("@Id", id);

                var doctor = _context.Doctors
                    .FromSqlRaw(sql, param)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (doctor == null)
                {
                    _logger.LogWarning("Doktor bulunamadı. ID: {Id}", id);
                    return ResponseGeneric<DoctorDto>.Error("Doktor bulunamadı.");
                }

                var dto = new DoctorDto
                {
                    Id = doctor.Id,
                    FullName = doctor.FullName,
                    Specialization = doctor.Specialization,
                    Phone = doctor.Phone,
                    DepartmentId = doctor.DepartmentId
                };

                _logger.LogInformation("Doktor başarıyla getirildi. ID: {Id}", id);
                return ResponseGeneric<DoctorDto>.Success(dto, "Doktor başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doktor getirilirken hata oluştu. ID: {Id}", id);
                return ResponseGeneric<DoctorDto>.Error("Doktor getirilirken hata oluştu.");
            }
        }

        public ResponseGeneric<DoctorDto> AddDoctor(DoctorDto doctorDto)
        {
            try
            {
                var sql = "EXEC Pr_Add_Doctor @FullName, @Specialization, @Phone, @DepartmentId";
                var parameters = new[]
                {
                    new SqlParameter("@FullName", doctorDto.FullName),
                    new SqlParameter("@Specialization", doctorDto.Specialization),
                    new SqlParameter("@Phone", doctorDto.Phone),
                    new SqlParameter("@DepartmentId", doctorDto.DepartmentId)
                };

                _context.Database.ExecuteSqlRaw(sql, parameters);
                _logger.LogInformation("Doktor başarıyla eklendi: {FullName}", doctorDto.FullName);

                return ResponseGeneric<DoctorDto>.Success(doctorDto, "Doktor başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doktor eklenirken hata oluştu.");
                return ResponseGeneric<DoctorDto>.Error("Doktor eklenirken hata oluştu: " + ex.Message);
            }
        }

        public ResponseGeneric<string> UpdateDoctor(int id, DoctorDto updatedDoctor)
        {
            try
            {
                var sql = "EXEC Pr_Update_Doctor @Id, @FullName, @Specialization, @Phone, @DepartmentId";
                var parameters = new[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@FullName", updatedDoctor.FullName ?? (object)DBNull.Value),
                    new SqlParameter("@Specialization", updatedDoctor.Specialization ?? (object)DBNull.Value),
                    new SqlParameter("@Phone", updatedDoctor.Phone ?? (object)DBNull.Value),
                    new SqlParameter("@DepartmentId", updatedDoctor.DepartmentId)
                };

                int affectedRows = _context.Database.ExecuteSqlRaw(sql, parameters);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Güncellenmek istenen doktor bulunamadı. ID: {Id}", id);
                    return ResponseGeneric<string>.Error("Doktor bulunamadı.");
                }

                _logger.LogInformation("Doktor başarıyla güncellendi. ID: {Id}", id);
                return ResponseGeneric<string>.Success("Doktor başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doktor güncellenirken hata oluştu. ID: {Id}", id);
                return ResponseGeneric<string>.Error("Doktor güncellenirken hata oluştu: " + ex.Message);
            }
        }

        public ResponseBase DeleteDoctorById(int id)
        {
            try
            {
                var sql = "EXEC Pr_Delete_Doctor @Id";
                var param = new SqlParameter("@Id", id);

                int affectedRows = _context.Database.ExecuteSqlRaw(sql, param);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Doktor başarıyla silindi. ID: {Id}", id);
                    return ResponseBase.Success("Doktor başarıyla silindi.");
                }
                else
                {
                    _logger.LogWarning("Silinmek istenen doktor bulunamadı. ID: {Id}", id);
                    return ResponseBase.Error("Doktor bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doktor silinirken hata oluştu. ID: {Id}", id);
                return ResponseBase.Error("Doktor silinirken hata oluştu: " + ex.Message);
            }
        }
    }
}
