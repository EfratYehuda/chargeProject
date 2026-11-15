using BLL.Services;
using DTO.AIDTO;
using Microsoft.AspNetCore.Mvc;
using System.ClientModel;
using System.Text.Json;


namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class chatController : ControllerBase
    {
        private readonly IAgentService _agentService;


        public chatController(IAgentService agentService)
        {
            _agentService = agentService;
        }


        [HttpPost("stream")]
        public async Task Stream(ChatRequestDto dto, CancellationToken ct)
        {
            Console.WriteLine($"[AI] 1. Request arrived ({dto.Messages.Count} messages)");
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";


            try
            {
                Console.WriteLine("[AI] 2. Calling AskAsync...");
                var answer = await _agentService.AskAsync(dto.Messages, ct);
                Console.WriteLine($"[AI] 3. Got answer ({answer?.Length ?? 0} chars)");


                var json = JsonSerializer.Serialize(new { text = answer ?? string.Empty });
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);


                await Response.WriteAsync("data: [DONE]\n\n", ct);
            }
            catch (ClientResultException ex)
            {
                var body = ex.GetRawResponse()?.Content?.ToString();
                Console.WriteLine($"=== GOOGLE AI ERROR ({ex.Status}) ===");
                Console.WriteLine(body);
                throw;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AI] Request was cancelled / timed out.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== UNEXPECTED AI ERROR: {ex.GetType().Name} ===");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}