using DTO.AIDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public interface IAgentService
    {
        Task<string> AskAsync(List<ChatMessageDto> history, CancellationToken ct);
        IAsyncEnumerable<string> AskStreamingAsync(List<ChatMessageDto> history, CancellationToken ct);
    }
}
