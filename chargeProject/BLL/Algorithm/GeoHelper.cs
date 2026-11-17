using DTO;
using System;

namespace BLL.Algorithm
{
    /// <summary>
    /// חישובים גיאוגרפיים: מרחק בין שתי נקודות ונקודת אמצע.
    /// מחלקה סטטית – אין לה מצב (state), רק פונקציות חישוב טהורות.
    /// </summary>
    public static class GeoHelper
    {
        private const double EarthRadiusKm = 6371;

        /// <summary>מרחק בקו אוויר (Haversine) בק"מ</summary>
        public static double AirDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        /// <summary>הערכת מרחק נסיעה בכביש: קו אוויר × מקדם כבישים (1.3)</summary>
        public static double RoadDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            return AirDistanceKm(lat1, lon1, lat2, lon2) * RouteRules.RoadFactor;
        }

        /// <summary>
        /// נקודת אמצע בין שתי נקודות – ממוצע פשוט של קווי רוחב ואורך.
        /// במרחקים של עשרות עד מאות ק"מ (כמו בישראל) הסטייה זניחה.
        /// </summary>
        public static (double Lat, double Lon) Midpoint(double lat1, double lon1, double lat2, double lon2)
        {
            return ((lat1 + lat2) / 2, (lon1 + lon2) / 2);
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
