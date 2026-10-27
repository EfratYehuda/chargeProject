using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public static class BookingStatuses
    {
        public const string Held = "Held";           // שריון זמני (15 דקות)
        public const string Confirmed = "Confirmed";
        public const string Charging = "Charging";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";     // השריון פג
        public const string Pattern = "^(Held|Confirmed|Charging|Completed|Cancelled|Expired)$";
    }

    /// <summary>חוקים עסקיים של הזמנות</summary>
    public static class BookingRules
    {
        public const int HoldMinutes = 15;
    }

    /// <summary>סטטוסים אפשריים של עמדה (בלוג הסטטוסים)</summary>
    public static class StationStatuses
    {
        public const string Available = "Available";
        public const string Occupied = "Occupied";
        public const string Reserved = "Reserved";
        public const string Faulty = "Faulty";       // דיווח תקלה
        public const string Pattern = "^(Available|Occupied|Reserved|Faulty)$";
    }

    /// <summary>תפקידי משתמש</summary>
    public static class UserRoles
    {
        public const string Driver = "Driver";
        public const string Owner = "Owner";
        public const string Admin = "Admin";
    }

    /// <summary>סוגי מטענים נתמכים</summary>
    public static class ChargerTypes
    {
        public const string CCS = "CCS";
        public const string Type2 = "Type2";
        public const string Pattern = "^(CCS|Type2)$";
    }
}
