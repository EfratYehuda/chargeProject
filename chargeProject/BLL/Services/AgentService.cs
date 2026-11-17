using BLL.Agent;
using DTO.AIDTO;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AgentService : IAgentService
    {
        private const string SystemPrompt = """
            אתה יועץ חכם למציאת עמדות טעינה לרכב חשמלי.
            ענה בעברית בלבד, בקצרה, באדיבות וללא סימני ** או מרקדאון.
            חוקים מחייבים:
            1. המלץ רק על עמדות שחזרו מהכלים FindBestStationsAsync, SearchStationsAsync או GetStationDetailsAsync. אם אין תוצאות, הצע בנימוס לחפש באזור סמוך.
            2. אם המשתמש ציין רחוב או עיר, העבר את הכתובת ישירות ל-FindBestStationsAsync בפרמטר address. השתמש ב-DistanceKm שחוזר מהכלי להצגת מרחק נסיעה (למשל "כ-1.8 ק"מ, כ-4 דקות נסיעה").
            3. שהייה של שעתיים ומעלה -> המלץ על Type2 / AC (חסכוני ושומר על הסוללה). לחץ זמן / סוללה נמוכה -> CCS / DC מהירה (50kW+).
            4. נמק בקצרה למה העמדה נבחרה (מרחק, הספק ומחיר).
            5. בסוף התשובה, לכל עמדה מומלצת, הוסף בשורה נפרדת את התגית המדויקת:
               [[STATION:ID:שם העמדה]]
            """;

        private static readonly HashSet<string> GreetingInputs = new(StringComparer.OrdinalIgnoreCase)
        {
            "היי", "הי", "שלום", "אהלן", "הלו", "בוקר טוב", "צהריים טובים", "ערב טוב", "מה נשמע", "מה קורה", "hi", "hello"
        };

        private static readonly HashSet<string> ThanksInputs = new(StringComparer.OrdinalIgnoreCase)
        {
            "תודה", "תודה רבה", "אחלה", "מעולה", "סבבה", "יופי", "ביי", "להתראות", "thanks"
        };

        private readonly IChatClient _chatClient;
        private readonly StationTools _stationTools;

        public AgentService(IChatClient chatClient, StationTools stationTools)
        {
            _chatClient = chatClient;
            _stationTools = stationTools;
        }

        private ChatOptions BuildOptions() => new ChatOptions
        {
            Tools = new List<AITool>
            {
                AIFunctionFactory.Create(_stationTools.FindBestStationsAsync),
                AIFunctionFactory.Create(_stationTools.SearchStationsAsync),
                AIFunctionFactory.Create(_stationTools.GetStationDetailsAsync)
            }
        };

        private List<ChatMessage> BuildMessages(List<ChatMessageDto> history)
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, SystemPrompt)
            };

            foreach (var m in history.TakeLast(4))
            {
                var role = m.Role == "ai" ? ChatRole.Assistant : ChatRole.User;
                messages.Add(new ChatMessage(role, m.Text));
            }

            return messages;
        }

        private static string? TryGetInstantReply(List<ChatMessageDto> history)
        {
            var lastUser = history.LastOrDefault(m => m.Role != "ai")?.Text;
            if (string.IsNullOrWhiteSpace(lastUser))
                return null;

            var rawLine = lastUser.Split('\n')[0].Trim().Trim('!', '.', '?', ',');

            if (GreetingInputs.Contains(rawLine))
                return "היי! ספר לי לאן אתה נוסע, כמה אחוזי סוללה יש לך או כמה זמן פנוי יש לך — ואמצא לך מיד את העמדה המשתלמת ביותר.";

            if (ThanksInputs.Contains(rawLine))
                return "בשמחה! נסיעה טובה ובטוחה ⚡";

            return null;
        }

        public async Task<string> AskAsync(List<ChatMessageDto> history, CancellationToken ct)
        {
            var instant = TryGetInstantReply(history);
            if (instant != null)
                return instant;

            var response = await _chatClient.GetResponseAsync(
                BuildMessages(history),
                BuildOptions(),
                cancellationToken: ct
            );
            return response.Text;
        }

        public async IAsyncEnumerable<string> AskStreamingAsync(
            List<ChatMessageDto> history,
            [EnumeratorCancellation] CancellationToken ct)
        {
            var instant = TryGetInstantReply(history);
            if (instant != null)
            {
                yield return instant;
                yield break;
            }

            var updates = _chatClient.GetStreamingResponseAsync(
                BuildMessages(history),
                BuildOptions(),
                cancellationToken: ct
            );

            await foreach (var update in updates)
            {
                if (!string.IsNullOrEmpty(update.Text))
                    yield return update.Text;
            }
        }
    }
}