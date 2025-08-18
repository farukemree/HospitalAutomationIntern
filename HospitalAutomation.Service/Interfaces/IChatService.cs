using HospitalAutomation.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Interfaces
{
    public interface IChatService
    {
        Task SaveMessage(ChatMessage message);
        Task<List<ChatMessage>> GetConversation(string userId1, string userId2);
    }
}
