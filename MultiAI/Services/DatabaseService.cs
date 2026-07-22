#nullable enable
using MultiAI.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MultiAI.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        public async Task InitAsync()
        {
            if (_db != null) return;

            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiAI", "history.db");
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<ChatSession>();
            await _db.CreateTableAsync<Message>();
        }

        public async Task<int> SaveSessionAsync(ChatSession session)
        {
            await InitAsync();
            if (_db == null) return 0;
            return await _db.InsertOrReplaceAsync(session);
        }

        public async Task<ChatSession?> GetSessionAsync(string sessionId)
        {
            await InitAsync();
            if (_db == null) return null;
            return await _db.Table<ChatSession>().Where(s => s.SessionId == sessionId).FirstOrDefaultAsync();
        }

        public async Task<List<ChatSession>> GetAllSessionsAsync()
        {
            await InitAsync();
            if (_db == null) return new List<ChatSession>();
            return await _db.Table<ChatSession>().OrderByDescending(s => s.LastUpdatedAt).ToListAsync();
        }

        public async Task<int> DeleteSessionAsync(string sessionId)
        {
            await InitAsync();
            if (_db == null) return 0;

            var msgs = await _db.Table<Message>().Where(m => m.SessionId == sessionId).ToListAsync();
            foreach (var m in msgs) await _db.DeleteAsync(m);

            return await _db.DeleteAsync<ChatSession>(sessionId);
        }

        public async Task<int> SaveMessageAsync(Message message)
        {
            await InitAsync();
            if (_db == null) return 0;
            if (message.Id > 0)
            {
                return await _db.UpdateAsync(message);
            }
            return await _db.InsertAsync(message);
        }

        public async Task<List<Message>> GetMessagesAsync(string sessionId)
        {
            await InitAsync();
            if (_db == null) return new List<Message>();
            return await _db.Table<Message>().Where(m => m.SessionId == sessionId).OrderBy(m => m.Timestamp).ToListAsync();
        }
    }
}
