using HospitalAutomation.API.Models;
using HospitalAutomation.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace HospitalAutomation.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        private string GenerateConversationId(string userId1, string userId2)
        {
            return string.Compare(userId1, userId2) < 0
                ? $"{userId1}_{userId2}"
                : $"{userId2}_{userId1}";
        }

        private string GetConversationGroupName(string conversationId)
        {
            return $"conv-{conversationId}";
        }

        // Kullanıcı conversation’a katılıyor
        public async Task JoinConversation(string conversationId)
        {
            var groupName = GetConversationGroupName(conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Mesaj gönder
        public async Task SendMessage(
            string conversationId,
            string senderId,
            string senderName,
            string receiverId,
            string receiverName,
            string message)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                SenderName = senderName,
                ReceiverId = receiverId,
                ReceiverName = receiverName,
                Message = message,
                ConversationId = conversationId,
                SentAt = DateTime.UtcNow
            };

            // Mesajı MongoDB'ye kaydet
            await _chatService.SaveMessage(chatMessage);

            var groupName = GetConversationGroupName(conversationId);

            // Mesajı aynı gruba gönder
            await Clients.Group(groupName)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }
}
