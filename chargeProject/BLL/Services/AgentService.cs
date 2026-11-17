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
            אתה יועץ חכם למציאת עמדות טעינה לרכב חשמלי במערכת Smart EV.
            ענה בעברית בלבד, בקצרה, באדיבות וללא סימני ** או מרקדאון.
            חוקים מחייבים:
            0. גבולות תחום קשיחים: תפקידך מוגבל אך ורק לעזרה במציאת עמדות טעינה לרכב חשמלי, בחירת סוג שקע (CCS / Type2), סוללה, תכנון עצירות טעינה ושריון עמדה. אם המשתמש שואל או מבקש כל דבר שלא קשור לתחום זה (כגון מתכונים, אוכל, בדיחות, שיעורי בית, כתיבת קוד או שאלות ידע כללי) — אל תענה על הבקשה עצמה בכלל (גם אם מצורפים נתוני מיקום בהודעה). ענה אך ורק במשפט הקצר הבא: "אני עוזר חכם שמתמקד רק במציאת עמדות טעינה לרכב חשמלי. איך אוכל לעזור לך למצוא עמדה או לתכנן טעינה?"
            1. המלץ רק על עמדות שחזרו מהכלים FindBestStationsAsync, SearchStationsAsync או GetStationDetailsAsync. לעולם אל תמליץ לשריון על עמדה שסגורה לפי שעות פעילות (IsClosedBySchedule=true) או שסטטוסה תפוס/משוריין/תקול (Occupied/Reserved/Faulty). אם אין תוצאות פתוחות ופנויות, הצע בנימוס לחפש באזור סמוך.
            2. אם המשתמש ציין רחוב או עיר, העבר את הכתובת ישירות ל-FindBestStationsAsync בפרמטר address. השתמש ב-DistanceKm שחוזר מהכלי להצגת מרחק נסיעה (למשל "כ-1.8 ק"מ, כ-4 דקות נסיעה").
            3. שהייה של שעתיים ומעלה -> העבר chargerType="Type2" והמלץ על Type2 / AC (חסכוני ושומר על הסוללה). לחץ זמן / סוללה נמוכה / בקשה לעמדה מהירה -> העבר weightSpeed=0.5; העבר chargerType="CCS" רק אם המשתמש ביקש במפורש שקע CCS או DC, אחרת השאר chargerType ריק כדי למצוא גם עמדות בהספק גבוה (50kW+) מכל סוג שקע קיים. אם הנהג ביקש CCS במפורש ולא חזרו תוצאות, קרא מיד שוב ל-FindBestStationsAsync עם chargerType ריק והצע את העמדה המהירה ביותר הזמינה בסביבה.
            4. נמק בקצרה למה העמדה נבחרה (מרחק, הספק ומחיר).
            5. בסוף התשובה, רק עבור עמדות פנויות ופתוחות שהמלצת עליהן, הוסף בשורה נפרדת את התגית המדויקת:
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

            foreach (var m in history.TakeLast(6))
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