using Microsoft.Extensions.AI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public sealed class FallbackChatClient : DelegatingChatClient
    {
        private readonly IReadOnlyList<(string Model, IChatClient Client)> _clients;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _cooldownUntil = new(StringComparer.OrdinalIgnoreCase);

        public FallbackChatClient(IReadOnlyList<(string Model, IChatClient Client)> clients)
            : base(clients[0].Client)
        {
            _clients = clients;
        }

        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            Exception? lastError = null;

            foreach (var (model, client) in GetOrderedClients())
            {
                try
                {
                    return await client.GetResponseAsync(messages, options, cancellationToken);
                }
                catch (Exception ex) when (ShouldFallback(ex, cancellationToken))
                {
                    lastError = ex;
                    _cooldownUntil[model] = DateTimeOffset.UtcNow.AddSeconds(60);
                    Console.WriteLine($"[AI] Model '{model}' failed ({ex.GetType().Name}), falling back to next model...");
                }
            }

            throw lastError ?? new InvalidOperationException("No Gemini models succeeded.");
        }

        public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Exception? lastError = null;

            foreach (var (model, client) in GetOrderedClients())
            {
                IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
                bool hasFirst;

                try
                {
                    enumerator = client.GetStreamingResponseAsync(messages, options, cancellationToken)
                        .GetAsyncEnumerator(cancellationToken);
                    hasFirst = await enumerator.MoveNextAsync();
                }
                catch (Exception ex) when (ShouldFallback(ex, cancellationToken))
                {
                    lastError = ex;
                    _cooldownUntil[model] = DateTimeOffset.UtcNow.AddSeconds(60);
                    Console.WriteLine($"[AI] Model '{model}' stream failed ({ex.GetType().Name}), falling back...");
                    if (enumerator != null)
                        await enumerator.DisposeAsync();
                    continue;
                }

                try
                {
                    if (hasFirst)
                    {
                        yield return enumerator.Current;
                        while (await enumerator.MoveNextAsync())
                        {
                            yield return enumerator.Current;
                        }
                    }
                    yield break;
                }
                finally
                {
                    await enumerator.DisposeAsync();
                }
            }

            if (lastError != null)
                throw lastError;
        }

        private List<(string Model, IChatClient Client)> GetOrderedClients()
        {
            var now = DateTimeOffset.UtcNow;
            var ready = _clients.Where(c => !_cooldownUntil.TryGetValue(c.Model, out var until) || until <= now).ToList();
            var cooled = _clients.Where(c => _cooldownUntil.TryGetValue(c.Model, out var until) && until > now).ToList();
            ready.AddRange(cooled);
            return ready;
        }

        private static bool ShouldFallback(Exception ex, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            var details = ex.ToString();
            if (details.Contains("401", StringComparison.OrdinalIgnoreCase) ||
                details.Contains("403", StringComparison.OrdinalIgnoreCase) ||
                details.Contains("API_KEY", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return details.Contains("503", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("429", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("404", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("ServerError", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("high demand", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("quota", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("overloaded", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                   details.Contains("TooManyRequests", StringComparison.OrdinalIgnoreCase);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 1; i < _clients.Count; i++)
                {
                    _clients[i].Client.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}