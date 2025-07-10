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

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(AppDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public ResponseGeneric<List<DepartmentDto>> GetAllDepartments()
    {
        try
        {
            var sql = "EXEC Pr_GetAllDepartments";
            var departments = _context.Departments.FromSqlRaw(sql).ToList();

            if (!departments.Any())
            {
                _logger.LogWarning("Hiç bölüm bulunamadı.");
                return ResponseGeneric<List<DepartmentDto>>.Error("Hiç bölüm bulunamadı.");
            }

            _logger.LogInformation("{Count} bölüm başarıyla getirildi.", departments.Count);

            var dtos = departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name
            }).ToList();

            return ResponseGeneric<List<DepartmentDto>>.Success(dtos, "Bölümler başarıyla getirildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölümler getirilirken hata oluştu.");
            return ResponseGeneric<List<DepartmentDto>>.Error("Bölümler getirilirken hata oluştu.");
        }
    }

    public ResponseGeneric<DepartmentDto> GetDepartmentById(int id)
    {
        try
        {
            var sql = "EXEC Pr_GetDepartmentById @Id";
            var param = new SqlParameter("@Id", id);

            var department = _context.Departments.FromSqlRaw(sql, param).AsEnumerable().FirstOrDefault();

            if (department == null)
            {
                _logger.LogWarning("Bölüm bulunamadı. ID: {Id}", id);
                return ResponseGeneric<DepartmentDto>.Error("Bölüm bulunamadı.");
            }

            _logger.LogInformation("Bölüm getirildi. ID: {Id}", id);

            var dto = new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name
            };

            return ResponseGeneric<DepartmentDto>.Success(dto, "Bölüm başarıyla getirildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölüm getirilirken hata oluştu. ID: {Id}", id);
            return ResponseGeneric<DepartmentDto>.Error("Bölüm getirilirken hata oluştu.");
        }
    }

    public Response AddDepartment(DepartmentDto dto)
    {
        try
        {
            var existing = _context.Departments.FirstOrDefault(d => d.Name == dto.Name);
            if (existing != null)
                return Response.Error("Bu isimde bölüm zaten mevcut.");

            var sql = "EXEC Pr_Add_Department @Name";
            var param = new SqlParameter("@Name", dto.Name);

            _context.Database.ExecuteSqlRaw(sql, param);
            _logger.LogInformation("Bölüm başarıyla eklendi: {Name}", dto.Name);
            return Response.Success("Bölüm başarıyla eklendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölüm eklenirken hata oluştu.");
            return Response.Error("Bölüm eklenirken hata oluştu.");
        }
    }

    public Response UpdateDepartment(int id, DepartmentDto dto)
    {
        try
        {
            var sql = "EXEC Pr_Update_Department @Id, @Name";
            var parameters = new[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Name", dto.Name ?? (object)DBNull.Value)
            };

            int rows = _context.Database.ExecuteSqlRaw(sql, parameters);
            if (rows == 0)
            {
                return Response.Error("Bölüm bulunamadı.");
            }

            _logger.LogInformation("Bölüm güncellendi. ID: {Id}", id);
            return Response.Success("Bölüm başarıyla güncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölüm güncellenirken hata oluştu.");
            return Response.Error("Bölüm güncellenirken hata oluştu.");
        }
    }

    public Response DeleteDepartmentById(int id)
    {
        try
        {
            var sql = "EXEC Pr_Delete_Department @Id";
            var param = new SqlParameter("@Id", id);

            int rows = _context.Database.ExecuteSqlRaw(sql, param);
            if (rows == 0)
            {
                _logger.LogWarning("Silinecek bölüm bulunamadı. ID: {Id}", id);
                return Response.Error("Bölüm bulunamadı.");
            }

            _logger.LogInformation("Bölüm silindi. ID: {Id}", id);
            return Response.Success("Bölüm başarıyla silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölüm silinirken hata oluştu.");
            return Response.Error("Bölüm silinirken hata oluştu.");
        }
    }
}
