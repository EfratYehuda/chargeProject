using BLL.Services;
using DTO.AIDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var messages = dto?.Messages ?? new List<ChatMessageDto>();
            Console.WriteLine($"[AI] 1. Request arrived ({messages.Count} messages)");

            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(35));

            try
            {
                Console.WriteLine("[AI] 2. Calling AskAsync...");
                var answer = await _agentService.AskAsync(messages, timeoutCts.Token);
                Console.WriteLine($"[AI] 3. Got answer ({answer?.Length ?? 0} chars)");

                await WriteSseMessageAsync(answer ?? string.Empty, ct);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                Console.WriteLine("[AI] Timed out after 35s.");
                await WriteSseMessageAsync(
                    "העוזר החכם מתעכב בתגובה. אפשר לנסות שוב בעוד רגע או לחפש ישירות דרך המפה.",
                    CancellationToken.None
                );
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AI] Client disconnected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== GOOGLE AI ERROR ({ex.GetType().Name}) ===");
                Console.WriteLine(ex.ToString());

                var details = ex.ToString();
                string userFriendlyMessage;

                if (details.Contains("429") ||
                    details.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase) ||
                    details.Contains("quota", StringComparison.OrdinalIgnoreCase))
                {
                    userFriendlyMessage =
                        "נגמרה מכסת הבקשות היומית של מפתח ה-AI. אפשר להחליף מפתח במשתני הסביבה או לחפש בינתיים דרך חיפוש העמדות והמפה.";
                }
                else if (details.Contains("401") ||
                         details.Contains("403") ||
                         details.Contains("API_KEY", StringComparison.OrdinalIgnoreCase))
                {
                    userFriendlyMessage =
                        "מפתח ה-AI בשרת אינו תקין או חסום. יש לבדוק את משתנה הסביבה GEMINI__APIKEY.";
                }
                else if (details.Contains("404") ||
                         details.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    userFriendlyMessage =
                        "שם מודל ה-AI שהוגדר בשרת לא נמצא. יש לבדוק את GEMINI__MODEL.";
                }
                else
                {
                    userFriendlyMessage =
                        "אירעה שגיאה זמנית בחיבור לעוזר החכם. אפשר לנסות שוב או לחפש עמדות במפה.";
                }

                await WriteSseMessageAsync(userFriendlyMessage, CancellationToken.None);
            }
        }

        private async Task WriteSseMessageAsync(string text, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(new { text });
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);

            await Response.WriteAsync("data: [DONE]\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}