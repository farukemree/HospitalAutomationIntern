using Azure;
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
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MedicalRecordService> _logger;
        private object _mapper;

        public MedicalRecordService(AppDbContext context, ILogger<MedicalRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ResponseGeneric<List<MedicalRecordDto>> GetAllMedicalRecords()
        {
            try
            {
                var sql = "EXEC Pr_GetAllMedicalRecords";
                var records = _context.MedicalRecords.FromSqlRaw(sql).ToList();

                if (!records.Any())
                {
                    _logger.LogWarning("Veritabanında hiç tıbbi kayıt bulunamadı.");
                    return ResponseGeneric<List<MedicalRecordDto>>.Error("Tıbbi kayıt bulunamadı.");
                }

                var dtoList = records.Select(r => new MedicalRecordDto
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    RecordDate = r.RecordDate,
                    Description = r.Diagnosis + (string.IsNullOrEmpty(r.Treatment) ? "" : " / " + r.Treatment)
                }).ToList();

                _logger.LogInformation("Tüm tıbbi kayıtlar başarıyla getirildi.");
                return ResponseGeneric<List<MedicalRecordDto>>.Success(dtoList, "Tıbbi kayıtlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tıbbi kayıtlar getirilirken hata oluştu.");
                return ResponseGeneric<List<MedicalRecordDto>>.Error("Tıbbi kayıtlar getirilirken hata oluştu.");
            }
        }
        public ResponseGeneric<List<MedicalRecordDto>> SearchMedicalRecordsByKeyword(string keyword)
        {
            try
            {
                var sql = "EXEC Pr_SearchMedicalRecordsByKeyword @Keyword";
                var param = new SqlParameter("@Keyword", keyword ?? "");

                var records = _context.MedicalRecords
                    .FromSqlRaw(sql, param)
                    .AsEnumerable()
                    .ToList();

                if (!records.Any())
                {
                    _logger.LogWarning($"Keyword '{keyword}' için tıbbi kayıt bulunamadı.");
                    return ResponseGeneric<List<MedicalRecordDto>>.Error("Tıbbi kayıt bulunamadı.");
                }

                var dtoList = records.Select(record => new MedicalRecordDto
                {
                    Id = record.Id,
                    PatientId = record.PatientId,
                    RecordDate = record.RecordDate,
                    Description = record.Diagnosis + (string.IsNullOrEmpty(record.Treatment) ? "" : " / " + record.Treatment)
                }).ToList();

                _logger.LogInformation($"Keyword '{keyword}' için tıbbi kayıtlar başarıyla getirildi.");
                return ResponseGeneric<List<MedicalRecordDto>>.Success(dtoList, "Tıbbi kayıtlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Keyword '{keyword}' ile tıbbi kayıtlar getirilirken hata oluştu.");
                return ResponseGeneric<List<MedicalRecordDto>>.Error("Tıbbi kayıtlar getirilirken hata oluştu.");
            }
        }



        public ResponseGeneric<MedicalRecordDto> GetMedicalRecordById(int id)
        {
            try
            {
                var sql = "EXEC Pr_GetMedicalRecordById @Id";
                var param = new SqlParameter("@Id", id);

                var record = _context.MedicalRecords
                    .FromSqlRaw(sql, param)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (record == null)
                {
                    _logger.LogWarning("Tıbbi kayıt bulunamadı. ID: {Id}", id);
                    return ResponseGeneric<MedicalRecordDto>.Error("Tıbbi kayıt bulunamadı.");
                }

                var dto = new MedicalRecordDto
                {
                    Id = record.Id,
                    PatientId = record.PatientId,
                    RecordDate = record.RecordDate,
                    Description = record.Diagnosis + (string.IsNullOrEmpty(record.Treatment) ? "" : " / " + record.Treatment)
                };

                _logger.LogInformation("Tıbbi kayıt başarıyla getirildi. ID: {Id}", id);
                return ResponseGeneric<MedicalRecordDto>.Success(dto, "Tıbbi kayıt başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tıbbi kayıt getirilirken hata oluştu. ID: {Id}", id);
                return ResponseGeneric<MedicalRecordDto>.Error("Tıbbi kayıt getirilirken hata oluştu.");
            }
        }

        public ResponseBase AddMedicalRecord(MedicalRecordDto dto)
        {
            try
            {
                var diagnosis = dto.Description;
                var treatment = ""; // Eğer varsa, dto'ya ekleyip buradan alabilirsin.

                var sql = "EXEC Pr_Add_MedicalRecord @PatientId, @Diagnosis, @Treatment, @RecordDate";
                var parameters = new[]
                {
                    new SqlParameter("@PatientId", dto.PatientId),
                    new SqlParameter("@Diagnosis", diagnosis ?? (object)DBNull.Value),
                    new SqlParameter("@Treatment", treatment ?? (object)DBNull.Value),
                    new SqlParameter("@RecordDate", dto.RecordDate)
                };

                _context.Database.ExecuteSqlRaw(sql, parameters);

                _logger.LogInformation("Tıbbi kayıt başarıyla eklendi.");
                return ResponseBase.Success("Tıbbi kayıt başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tıbbi kayıt eklenirken hata oluştu.");
                return ResponseBase.Error($"Tıbbi kayıt eklenirken hata oluştu: {ex.Message}");
            }
        }

        public ResponseBase UpdateMedicalRecord(int id, MedicalRecordDto dto)
        {
            try
            {
                var diagnosis = dto.Description;
                var treatment = "";

                var sql = "EXEC Pr_Update_MedicalRecord @Id, @PatientId, @Diagnosis, @Treatment, @RecordDate";
                var parameters = new[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@PatientId", dto.PatientId),
                    new SqlParameter("@Diagnosis", diagnosis ?? (object)DBNull.Value),
                    new SqlParameter("@Treatment", treatment ?? (object)DBNull.Value),
                    new SqlParameter("@RecordDate", dto.RecordDate)
                };

                int affectedRows = _context.Database.ExecuteSqlRaw(sql, parameters);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Güncellenmek istenen tıbbi kayıt bulunamadı. ID: {Id}", id);
                    return ResponseBase.Error("Tıbbi kayıt bulunamadı.");
                }

                _logger.LogInformation("Tıbbi kayıt başarıyla güncellendi. ID: {Id}", id);
                return ResponseBase.Success("Tıbbi kayıt başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tıbbi kayıt güncellenirken hata oluştu. ID: {Id}", id);
                return ResponseBase.Error($"Tıbbi kayıt güncellenirken hata oluştu: {ex.Message}");
            }
        }
        public ResponseGeneric<List<MedicalRecordDto>> GetMedicalRecordsByPatientId(int patientId)
        {
            try
            {
                var sql = "EXEC Pr_GetMedicalRecordsByPatientId @PatientId";
                var param = new SqlParameter("@PatientId", patientId);

                var medicalRecords = _context.MedicalRecords
                    .FromSqlRaw(sql, param)
                    .ToList();

                if (!medicalRecords.Any())
                {
                    _logger.LogWarning($"PatientId {patientId} için tıbbi kayıt bulunamadı.");
                    return ResponseGeneric<List<MedicalRecordDto>>.Success(new List<MedicalRecordDto>(), "Henüz tıbbi kayıt bulunmamaktadır.");
                }

                var dtoList = medicalRecords.Select(mr => new MedicalRecordDto
                {
                    Id = mr.Id,
                    PatientId = mr.PatientId,
                    RecordDate = mr.RecordDate,
                    Description = $"Tanı: {mr.Diagnosis} | Tedavi: {mr.Treatment}"
                }).ToList();

                _logger.LogInformation($"PatientId {patientId} için tıbbi kayıtlar başarıyla getirildi.");
                return ResponseGeneric<List<MedicalRecordDto>>.Success(dtoList, "Tıbbi kayıtlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasta tıbbi kayıtları getirilirken hata oluştu.");
                return ResponseGeneric<List<MedicalRecordDto>>.Error("Veriler alınırken hata oluştu.");
            }
        }


        public ResponseBase DeleteMedicalRecordById(int id)
        {
            try
            {
                var sql = "EXEC Pr_Delete_MedicalRecord @Id";
                var param = new SqlParameter("@Id", id);

                int affectedRows = _context.Database.ExecuteSqlRaw(sql, param);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Silinmek istenen tıbbi kayıt bulunamadı. ID: {Id}", id);
                    return ResponseBase.Error("Tıbbi kayıt bulunamadı.");
                }

                _logger.LogInformation("Tıbbi kayıt başarıyla silindi. ID: {Id}", id);
                return ResponseBase.Success("Tıbbi kayıt başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tıbbi kayıt silinirken hata oluştu. ID: {Id}", id);
                return ResponseBase.Error($"Tıbbi kayıt silinirken hata oluştu: {ex.Message}");
            }
        }
    }
}
