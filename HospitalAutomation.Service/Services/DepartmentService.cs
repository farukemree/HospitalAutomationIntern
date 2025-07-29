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
using StackExchange.Redis;
public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentService> _logger;
    private readonly IRedisCacheService _cacheService;
    public DepartmentService(AppDbContext context, ILogger<DepartmentService> logger, IRedisCacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
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
                Name = d.Name,
            }).ToList();

            return ResponseGeneric<List<DepartmentDto>>.Success(dtos, "Bölümler başarıyla getirildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölümler getirilirken hata oluştu.");
            return ResponseGeneric<List<DepartmentDto>>.Error("Bölümler getirilirken hata oluştu.");
        }
    }
    public async Task<ResponseGeneric<List<DepartmentDto>>> GetAllDepartmentsWithDescriptionsAsync()
    {
        try
        {
            const string cacheKey = "departments_with_descriptions";

            var cached = await _cacheService.GetAsync<List<DepartmentDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Departman verileri Redis'ten alındı.");
                return ResponseGeneric<List<DepartmentDto>>.Success(cached, "Redis üzerinden getirildi.");
            }

            var sql = "EXEC GetAllDepartmentsWithDescriptions";
            var departments = _context.Departments.FromSqlRaw(sql).ToList();

            if (!departments.Any())
            {
                _logger.LogWarning("Hiç bölüm bulunamadı.");
                return ResponseGeneric<List<DepartmentDto>>.Error("Hiç bölüm bulunamadı.");
            }

            var dtos = departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description
            }).ToList();

            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromHours(1));

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

            var sql = "EXEC Pr_Add_Department @Name, @Description";
            var parameters = new[]
            {
            new SqlParameter("@Name", dto.Name),
            new SqlParameter("@Description", (object?)dto.Description ?? DBNull.Value)
        };

            _context.Database.ExecuteSqlRaw(sql, parameters);
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
            var sql = "EXEC Pr_Update_Department @Id, @Name, @Description";
            var parameters = new[]
            {
            new SqlParameter("@Id", id),
            new SqlParameter("@Name", dto.Name ?? (object)DBNull.Value),
            new SqlParameter("@Description", dto.Description ?? (object)DBNull.Value)
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
