using System;
using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public string SenderRole { get; set; } // "User" hoặc "Admin"
        public string Message { get; set; }

        // Thêm 2 trường lưu Media
        public string MessageType { get; set; } = "Text"; // "Text", "Image", "Video"
        public string MediaUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}