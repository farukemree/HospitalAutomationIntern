using HospitalAutomation.API.DataAccess.Mongo;
using HospitalAutomation.API.Models;
using HospitalAutomation.Service.Interfaces;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Services
{
    public class ChatService : IChatService
    {
        private readonly MongoDbContext _context;

        public ChatService(MongoDbContext context)
        {
            _context = context;
        }

        // Ortak ConversationId oluşturucu (ör: 1_5 veya 5_1 her zaman aynı sonucu verir)
        private string GenerateConversationId(string userId1, string userId2)
        {
            return string.Compare(userId1, userId2) < 0
                ? $"{userId1}_{userId2}"
                : $"{userId2}_{userId1}";
        }

        public async Task SaveMessage(ChatMessage message)
        {
            // ConversationId otomatik ata
            message.ConversationId = GenerateConversationId(message.SenderId, message.ReceiverId);

            await _context.ChatMessages.InsertOneAsync(message);
        }

        public async Task<List<ChatMessage>> GetConversation(string userId1, string userId2)
        {
            var conversationId = GenerateConversationId(userId1, userId2);

            // Artık ConversationId üzerinden çok daha basit sorgu yapabiliriz
            var filter = Builders<ChatMessage>.Filter.Eq(m => m.ConversationId, conversationId);

            return await _context.ChatMessages
                .Find(filter)
                .SortBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}
