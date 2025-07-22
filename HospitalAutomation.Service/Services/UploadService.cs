using HospitalAutomation.DataAccess.Context;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.DataAccess.Models;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


public class UploadService : IUploadService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UploadService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;



    public UploadService(AppDbContext context, ILogger<UploadService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /*
    public string GenerateFileKey() 
    {
        var newFileKey = new Guid();
        return newFileKey.ToString();
    }
    */
    private bool IsValidImage(string extension)
    {
        return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".jfif";
    }

    public async Task<ResponseGeneric<string>> UploadUserImageAsync(IFormFile file, string fileKeyFromClient)
    {
        if (file == null || file.Length == 0)
            return ResponseGeneric<string>.Error("Dosya boş.");

        string fileName = Path.GetFileName(file.FileName);
        string fileExtension = Path.GetExtension(fileName).ToLower();

        if (!IsValidImage(fileExtension))
            return ResponseGeneric<string>.Error("Sadece resim dosyaları yüklenebilir (.jpg, .jpeg, .png, .jfif).");

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return ResponseGeneric<string>.Error("Kullanıcı bilgisi bulunamadı.");
        }

        string generatedFileKey = Guid.NewGuid().ToString();
        string guidedFileName = generatedFileKey + fileExtension;

        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Users");
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        string filePath = Path.Combine(uploadPath, guidedFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        try
        {
            var parameters = new[]
            {
            new SqlParameter("@FileOriginalName", fileName),
            new SqlParameter("@FileGuidedName", guidedFileName),
            new SqlParameter("@FileKey", Guid.Parse(generatedFileKey)),
            new SqlParameter("@FilePath", filePath)
        };

            // Upload image bilgisi için SP çağrısı
            await _context.Database.ExecuteSqlRawAsync("EXEC Sp_UploadUserImage @FileOriginalName, @FileGuidedName, @FileKey, @FilePath", parameters);

            // Kullanıcı tablosunda FileKey güncellemesi için SP çağrısı
            var userParameters = new[]
            {
            new SqlParameter("@UserId", userId),
            new SqlParameter("@FileKey", generatedFileKey)
        };

            await _context.Database.ExecuteSqlRawAsync("EXEC Sp_UpdateUserFileKey @UserId, @FileKey", userParameters);
        }
        catch (Exception ex)
        {
            var error = ex.InnerException?.Message ?? ex.Message;
            throw new Exception("Veritabanı kaydı sırasında hata oluştu: " + error);
        }

        _logger.LogInformation("Kullanıcı resmi başarıyla yüklendi ve kullanıcıya atandı.");

        return ResponseGeneric<string>.Success(generatedFileKey, "Dosya başarıyla yüklendi.");
    }

    public string GetUserImageUrl(string fileKey)
    {
        if (string.IsNullOrEmpty(fileKey))
            return null;

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Users");

        var files = Directory.GetFiles(uploadsFolder, fileKey + ".*");
        if (files.Length == 0)
            return null;

        string fileName = Path.GetFileName(files[0]);

        string baseUrl = "http://localhost:5073";
        string url = $"{baseUrl}/Uploads/Users/{fileName}";

        return url;
    }
    

}


