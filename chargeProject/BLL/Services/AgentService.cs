using BLL.Agent;
using DTO.AIDTO;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AgentService : IAgentService
    {
        private const string SystemPrompt = """
            אתה יועץ ועוזר אישי חכם ומומחה לאיתור עמדות הטענה לרכב חשמלי.
            ענה תמיד בעברית בלבד, בסבר פנים יפות, באדיבות, בקצרה ובאופן מקצועי וממוקד.
            כתוב טקסט נקי וברור ללא סימני עיצוב כמו כוכביות (אל תכתוב ** סביב מילים).
            תפקידך לסייע לנהגים ברכב חשמלי למצוא את עמדת ההטענה האופטימלית ביותר עבורם לפי העדפותיהם, מיקומם ואילוצי הזמן שלהם.

            הנחיות פעולה מחייבות:
            1. איסור מוחלט על המצאת עמדות (Anti-Hallucination):
               אל תמציא עמדות, כתובות או נתונים שלא קיימים! כל עמדה שתוצג למשתמש חייבת להגיע אך ורק מתוצאות הכלים SearchStationsAsync, FindBestStationsAsync או GetStationDetailsAsync. אם לא נמצאה עמדה, ציין זאת בנימוס והצע לחפש באזור סמוך.
            2. פענוח מיקומים וחישוב מרחק נהיגה (לא הליכה!):
               כאשר המשתמש מציין כתובת או רחוב, השתמש בכלי GetCoordinatesFromAddressAsync כדי לפענח את הכתובת לנקודת GPS. לאחר מכן אתר עמדות באזור והשתמש ב-CalculateDistanceKm כדי לחשב ולהציג את המרחק בקילומטרים או במטרים (למשל: "כ-230 מטר" או "כ-1.8 ק"מ, כ-4 דקות נסיעה"). לעולם אל תכתוב "דקות הליכה" — המשתמש נוהג ברכב!
            3. התאמה חכמה לאילוצי זמן וסוגי מטענים:
               - שהייה ארוכה (שעתיים ומעלה, פגישה, קניות או עבודה): המלץ על עמדת טעינה רגילה Type2 / AC (עד 22kW). הסבר לנהג שזה חוסך עלויות ושומר על חיי הסוללה.
               - חיפוש מהיר / לחץ זמן / סוללה קריטית (מתחת לשעה): המלץ על עמדת CCS / DC מהירה (50kW ומעלה) שתטעין את הרכב בזמן קצר.
            4. נימוק ההמלצה:
               נמק תמיד בקצרה מדוע העמדה שבחרת היא הטובה ביותר עבור הנהג (קרבה פיזית, מהירות טעינה, זמינות ומחיר לקוט"ש).
            5. תגית פעולה לאפליקציה:
               עבור כל עמדה ספציפית מהכלים שאתה ממליץ עליה בתשובתך, הוסף בסוף התשובה בשורה נפרדת תגית בפורמט המדויק:
               [[STATION:ID:שם העמדה]]
               (לדוגמה: [[STATION:3:עמדת הטענה נוחה]]).
               החלף את ID במזהה המספרי האמיתי מהכלי ואת שם העמדה בשם האמיתי מהכלי.
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