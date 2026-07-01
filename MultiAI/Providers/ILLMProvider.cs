#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MultiAI.Models;

namespace MultiAI.Providers
{
    public interface ILLMProvider
    {
        string Name { get; }
        void Initialize(string apiKey, string model);
        Task<List<string>> GetAvailableModelsAsync(string apiKey);
        Task<string> SendMessageAsync(string message, List<Message> history);
        IAsyncEnumerable<string> StreamMessageAsync(string message, List<Message> history, CancellationToken cancellationToken = default);
    }
}
