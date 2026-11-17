using BLL.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Helpers
{
    public static class StationScheduleHelper
    {
        private static TimeZoneInfo GetIsraelTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jerusalem");
                }
                catch (TimeZoneNotFoundException)
                {
                    return TimeZoneInfo.Local;
                }
            }
        }

        public static DateTime GetIsraelNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetIsraelTimeZone());
        }

        public static DateTime ToIsraelTime(DateTime dt)
        {
            var tz = GetIsraelTimeZone();
            if (dt.Kind == DateTimeKind.Utc)
                return TimeZoneInfo.ConvertTimeFromUtc(dt, tz);

            if (dt.Kind == DateTimeKind.Local)
                return TimeZoneInfo.ConvertTime(dt, tz);

            return dt;
        }

        public static List<int> ParseBlockedHours(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<int>();

            return raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var h) ? h : -1)
                .Where(h => h >= 0 && h <= 23)
                .Distinct()
                .OrderBy(h => h)
                .ToList();
        }

        public static string? SerializeBlockedHours(IEnumerable<int>? hours)
        {
            if (hours == null)
                return null;

            var valid = hours
                .Where(h => h >= 0 && h <= 23)
                .Distinct()
                .OrderBy(h => h)
                .ToList();

            return valid.Count == 0 ? null : string.Join(",", valid);
        }

        public static bool IsClosedNow(string? raw)
        {
            var blocked = ParseBlockedHours(raw);
            if (blocked.Count == 0)
                return false;

            var currentHour = GetIsraelNow().Hour;
            return blocked.Contains(currentHour);
        }

        public static int? GetNextOpenHour(string? raw)
        {
            var blocked = ParseBlockedHours(raw);
            if (blocked.Count == 0 || blocked.Count >= 24)
                return null;

            var currentHour = GetIsraelNow().Hour;
            if (!blocked.Contains(currentHour))
                return null;

            for (int offset = 1; offset <= 23; offset++)
            {
                int candidate = (currentHour + offset) % 24;
                if (!blocked.Contains(candidate))
                    return candidate;
            }

            return null;
        }

        /// <summary>
        /// בודק שהשעה הנוכחית פתוחה ושזמן השריון המאושר לא גולש לשעה חסומה
        /// </summary>
        public static void ValidateBookingWindow(string? blockedHoursRaw, DateTime israelNow, DateTime holdUntilIsrael)
        {
            var blocked = ParseBlockedHours(blockedHoursRaw);
            if (blocked.Count == 0)
                return;

            if (blocked.Contains(israelNow.Hour))
            {
                var nextOpen = GetNextOpenHour(blockedHoursRaw);
                if (nextOpen.HasValue)
                    throw new BusinessRuleException($"העמדה סגורה כעת ותיפתח בשעה {nextOpen.Value:00}:00");

                throw new BusinessRuleException("העמדה סגורה כעת לפי שעות הפעילות שהוגדרו");
            }

            // בדיקה משעה לשעה עד לסיום חלון השריון (למשל אם שריון ב-22:50 גולש ל-23:05)
            var cursor = new DateTime(israelNow.Year, israelNow.Month, israelNow.Day, israelNow.Hour, 0, 0);
            var endHourStart = new DateTime(holdUntilIsrael.Year, holdUntilIsrael.Month, holdUntilIsrael.Day, holdUntilIsrael.Hour, 0, 0);

            while (cursor <= endHourStart)
            {
                if (blocked.Contains(cursor.Hour))
                {
                    throw new BusinessRuleException($"לא ניתן לשריין כעת — זמן השריון גולש לשעת הסגירה ({cursor.Hour:00}:00)");
                }
                cursor = cursor.AddHours(1);
            }
        }
    }
}
