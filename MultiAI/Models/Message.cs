#nullable enable
using SQLite;
using System;

namespace MultiAI.Models
{
    public class Message
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string SessionId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "user" or "ai"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Ignore]
        public string FormattedTimestamp => Timestamp.ToString("g");
    }
}
