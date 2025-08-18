using HospitalAutomation.API.DataAccess.Mongo;
using HospitalAutomation.API.Models;
using HospitalAutomation.DataAccess.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // SQL için
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly MongoDbContext _mongoDb;
    private readonly AppDbContext _sqlDb; // User tablosu için SQL DbContext

    public ChatController(MongoDbContext mongoDb, AppDbContext sqlDb)
    {
        _mongoDb = mongoDb;
        _sqlDb = sqlDb;
    }


    [HttpGet("GetMessages")]
    public async Task<ActionResult<List<ChatMessage>>> GetMessages(
     [FromQuery] string userId,
     [FromQuery] string? otherUserId = null)
        {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId boş olamaz.");

            FilterDefinition<ChatMessage> filter;

            if (!string.IsNullOrEmpty(otherUserId))
            {
                // ConversationId üzerinden filtre
                string conversationId = GenerateConversationId(userId, otherUserId);
                filter = Builders<ChatMessage>.Filter.Eq(m => m.ConversationId, conversationId);
            }
            else
            {
                // Kullanıcıya ait tüm mesajları getir
                filter = Builders<ChatMessage>.Filter.Or(
                    Builders<ChatMessage>.Filter.Eq(m => m.SenderId, userId),
                    Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, userId)
                );
            }

            var messages = await _mongoDb.ChatMessages
                .Find(filter)
                .SortBy(m => m.SentAt)
                .ToListAsync();

            // Her mesaj için sender ve receiver isimlerini async olarak al
            var tasks = messages.Select(async msg =>
            {
                msg.SenderName = await GetUserNameById(msg.SenderId) ?? "";
                msg.ReceiverName = await GetUserNameById(msg.ReceiverId) ?? "";
            });

            await Task.WhenAll(tasks);

            return Ok(messages);
        }
        catch (Exception ex)
        {
            // Burada loglama yapabilirsin, örn: _logger.LogError(ex, "Mesajlar alınamadı");
            return StatusCode(500, $"Mesajlar alınırken hata oluştu: {ex.Message}");
        }
    }



    [HttpGet("GetMessagesForDoctor")]
    public async Task<ActionResult<List<ChatMessage>>> GetMessagesForDoctor([FromQuery] string doctorId)
    {
        if (string.IsNullOrEmpty(doctorId))
            return BadRequest("DoctorId boş olamaz");

        // Hem gelen hem giden mesajlar
        var filter = Builders<ChatMessage>.Filter.Or(
            Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, doctorId),
            Builders<ChatMessage>.Filter.Eq(m => m.SenderId, doctorId)
        );

        var messages = await _mongoDb.ChatMessages
            .Find(filter)
            .SortBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }





    private async Task<string> GetUserNameById(string userId)
    {
        try
        {
            if (!int.TryParse(userId, out var id))
                return "Bilinmiyor";

            // Sadece gerekli alanları çekiyoruz, Include kullanmadan
            var user = await _sqlDb.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    DoctorFullName = u.Doctor != null ? u.Doctor.FullName : null,
                    PatientFullName = u.Patient != null ? u.Patient.FullName : null
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return "Bilinmiyor";

            // Öncelik: Doctor > Patient > Username
            return user.DoctorFullName ?? user.PatientFullName ?? user.Username ?? "Bilinmiyor";
        }
        catch (Exception ex)
        {
            // Loglama yapabilirsin
            Console.WriteLine("GetUserNameById hatası: " + ex.Message);
            return "Bilinmiyor";
        }
    }



    private string GenerateConversationId(string userId1, string userId2)
    {
        return string.Compare(userId1, userId2) < 0
            ? $"{userId1}_{userId2}"
            : $"{userId2}_{userId1}";
    }

}
