using MultiAI.Models;
using MultiAI.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MultiAI.Tests
{
    public class DatabaseServiceTests
    {
        [Fact]
        public async Task SaveSessionAsync_ShouldInsertOrUpdateSession()
        {
            var db = new DatabaseService();
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid().ToString(),
                Title = "Test Unit Chat",
                Provider = "Google Gemini",
                Model = "gemini-1.5-flash",
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            };

            int result = await db.SaveSessionAsync(session);
            Assert.True(result > 0);

            var retrieved = await db.GetSessionAsync(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Unit Chat", retrieved.Title);
            Assert.Equal("Google Gemini", retrieved.Provider);

            // Clean up
            await db.DeleteSessionAsync(session.SessionId);
        }

        [Fact]
        public async Task SaveAndGetMessagesAsync_ShouldPersistMessagesInOrder()
        {
            var db = new DatabaseService();
            string sessionId = Guid.NewGuid().ToString();

            var msg1 = new Message
            {
                SessionId = sessionId,
                Role = "user",
                Content = "Hello AI",
                Timestamp = DateTime.Now.AddSeconds(-2)
            };

            var msg2 = new Message
            {
                SessionId = sessionId,
                Role = "Google Gemini",
                Content = "Hello user!",
                Timestamp = DateTime.Now
            };

            await db.SaveMessageAsync(msg1);
            await db.SaveMessageAsync(msg2);

            var messages = await db.GetMessagesAsync(sessionId);
            Assert.Equal(2, messages.Count);
            Assert.Equal("Hello AI", messages[0].Content);
            Assert.Equal("Hello user!", messages[1].Content);

            // Clean up
            await db.DeleteSessionAsync(sessionId);
        }
    }
}
