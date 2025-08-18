using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HospitalAutomation.API.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string SenderId { get; set; }
        public string SenderName { get; set; }


        public string ReceiverName { get; set; } 

        // Hasta veya Doktor Id
        public string ReceiverId { get; set; }
        public string ConversationId { get; set; }


        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}

