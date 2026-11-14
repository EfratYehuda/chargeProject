using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace BLL.Agent
{
    // DTO לקליטת ה-JSON מה-API החיצוני
    public class NominatimResponse
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = "";

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = "";

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = "";
    }

    public record CoordinatesResult(double Latitude, double Longitude, string FullAddress);

    public class LocationTools
    {
        private readonly HttpClient _http;

        public LocationTools(HttpClient http)
        {
            _http = http;
        }

        [Description("מפענח כתובת טקסטואלית בישראל (עיר ורחוב) לקואורדינטות GPS של קו אורך ורוחב.")]
        public async Task<CoordinatesResult?> GetCoordinatesFromAddressAsync(
            [Description("שם הרחוב והעיר, למשל: רוטשילד פתח תקווה")] string address,
            CancellationToken ct)
        {
            try
            {
                // פנייה ל-API החיצוני של OpenStreetMap Nominatim
                var url = $"search?q={Uri.EscapeDataString(address + ", ישראל")}&format=json&limit=1";
                var results = await _http.GetFromJsonAsync<List<NominatimResponse>>(url, ct);
                var first = results?.FirstOrDefault();

                if (first != null &&
                    double.TryParse(first.Lat, out var lat) &&
                    double.TryParse(first.Lon, out var lon))
                {
                    return new CoordinatesResult(lat, lon, first.DisplayName);
                }

                return null;
            }
            catch
            {
                return null; // אם יש שגיאת רשת, המודל ימשיך עם הכתובת הטקסטואלית
            }
        }

        [Description("מחשב מרחק אווירי בקילומטרים בין שתי נקודות GPS (קו אורך ורוחב).")]
        public double CalculateDistanceKm(
            [Description("קו רוחב נקודה 1")] double lat1,
            [Description("קו אורך נקודה 1")] double lon1,
            [Description("קו רוחב נקודה 2")] double lat2,
            [Description("קו אורך נקודה 2")] double lon2)
        {
            var r = 6371; // רדיוס כדור הארץ בק"מ
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round(r * c, 2);
        }
    }
}
