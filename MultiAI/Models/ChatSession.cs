#nullable enable
using SQLite;
using System;

namespace MultiAI.Models
{
    public class ChatSession
    {
        [PrimaryKey]
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = "New Chat";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
