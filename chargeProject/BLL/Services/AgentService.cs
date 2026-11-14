using BLL.Agent; // כדי שיכיר את StationTools ו-LocationTools
using DTO.AIDTO;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AgentService : IAgentService
    {
        private const string SystemPrompt = """
            אתה יועץ ועוזר אישי חכם ומומחה לאיתור עמדות הטענה לרכב חשמלי.
            ענה תמיד בעברית בלבד, בסבר פנים יפות, באדיבות, בקצרה ובאופן מקצועי וממוקד.

            תפקידך לסייע לנהגים למצוא את עמדת ההטענה האופטימלית ביותר עבורם לפי העדפותיהם, מיקומם ואילוצי הזמן שלהם.

            הנחיות פעולה מחייבות:
            1. איסור מוחלט על המצאת עמדות (Anti-Hallucination):
               אל תמציא עמדות, כתובות או נתונים שלא קיימים! כל עמדה שתוצג למשתמש חייבת להגיע אך ורק מתוצאות הכלים SearchStationsAsync, GetStationsNearbyAsync או GetStationDetailsAsync. אם לא נמצאה עמדה, ציין זאת בנימוס והצע לחפש באזור סמוך.

            2. פענוח מיקומים וחישוב מרחק:
               כאשר המשתמש מציין כתובת או רחוב (כגון 'רחוב רוטשילד בפתח תקווה'), השתמש בכלי GetCoordinatesFromAddressAsync כדי לפענח את הכתובת לנקודת GPS. לאחר מכן אתר עמדות באזור והשתמש ב-CalculateDistanceKm כדי לחשב ולהציג לנהג את המרחק המשוער (למשל: "כ-300 מטר ממך").

            3. התאמה חכמה לאילוצי זמן וסוגי מטענים:
               - שהייה ארוכה (שעתיים ומעלה, פגישה, קניות או עבודה): המלץ על עמדת טעינה רגילה AC (עד 22kW). הסבר לנהג שזה חוסך עלויות כספיות, שומר על חיי הסוללה ומונע דמי תפוסה/קנסות על תפיסת עמדה מהירה לאחר סיום הטעינה.
               - חיפוש מהיר / לחץ זמן / סוללה קריטית (מתחת לשעה): המלץ על עמדת DC מהירה או אולטרה-מהירה (50kW ומעלה) שתטעין את הרכב בזמן קצר.

            4. נימוק ההמלצה:
               נמק תמיד בקצרה מדוע העמדה שבחרת היא הטובה ביותר עבור הנהג (שקלול של קרבה פיזית, מהירות ההטענה הנדרשת, זמינות ומחיר לקוט"ש).
            """;

        private readonly IChatClient _chatClient;
        private readonly StationTools _stationTools;
        private readonly LocationTools _locationTools;

        public AgentService(
            IChatClient chatClient,
            StationTools stationTools,
            LocationTools locationTools)
        {
            _chatClient = chatClient;
            _stationTools = stationTools;
            _locationTools = locationTools;
        }

        // חיבור כל הכלים שהסוכן מוסמך להפעיל
        private ChatOptions BuildOptions() => new ChatOptions
        {
            Tools = new List<AITool>
            {
                AIFunctionFactory.Create(_stationTools.SearchStationsAsync),
                AIFunctionFactory.Create(_stationTools.GetStationDetailsAsync),
                AIFunctionFactory.Create(_locationTools.GetCoordinatesFromAddressAsync),
                AIFunctionFactory.Create(_locationTools.CalculateDistanceKm),
                AIFunctionFactory.Create(_stationTools.FindBestStationsAsync)
            }
        };

        private List<ChatMessage> BuildMessages(List<ChatMessageDto> history)
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, SystemPrompt)
            };

            foreach (var m in history)
            {
                var role = m.Role == "ai" ? ChatRole.Assistant : ChatRole.User;
                messages.Add(new ChatMessage(role, m.Text));
            }

            return messages;
        }

        public async Task<string> AskAsync(List<ChatMessageDto> history, CancellationToken ct)
        {
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