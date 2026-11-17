using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Agent
{
    // DTO לקליטת ה-JSON מ-OpenStreetMap Nominatim
    public class NominatimResponse
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = "";

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = "";

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = "";
    }

    // DTO לקליטת ה-JSON מ-ArcGIS כגיבוי מדויק לכתובות בישראל
    public class ArcGisGeocodeResponse
    {
        [JsonPropertyName("candidates")]
        public List<ArcGisCandidate> Candidates { get; set; } = new();
    }

    public class ArcGisCandidate
    {
        [JsonPropertyName("address")]
        public string Address { get; set; } = "";

        [JsonPropertyName("location")]
        public ArcGisLocation? Location { get; set; }
    }

    public class ArcGisLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
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
            [Description("שם הרחוב והעיר, למשל: רוטשילד 50 פתח תקווה")] string address,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            var trimmed = address.Trim();
            var queryWithCountry = trimmed.Contains("ישראל", StringComparison.OrdinalIgnoreCase)
                ? trimmed
                : $"{trimmed}, ישראל";

            // ניסיון 1: OpenStreetMap Nominatim
            try
            {
                var url =
                    $"search?q={Uri.EscapeDataString(queryWithCountry)}&format=json&countrycodes=il&accept-language=he&limit=1";
                var results = await _http.GetFromJsonAsync<List<NominatimResponse>>(url, ct);
                var first = results?.FirstOrDefault();

                if (first != null &&
                    double.TryParse(first.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                    double.TryParse(first.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
                {
                    return new CoordinatesResult(lat, lon, first.DisplayName);
                }
            }
            catch
            {
                // ממשיכים לגיבוי ArcGIS
            }

            // ניסיון 2: ArcGIS Geocoding (מדויק במיוחד לרחוב + מספר בית בישראל)
            try
            {
                var arcUrl =
                    $"https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates" +
                    $"?SingleLine={Uri.EscapeDataString(trimmed)}&countryCode=ISR&langCode=he&maxLocations=1&f=json";
                var arcResponse = await _http.GetFromJsonAsync<ArcGisGeocodeResponse>(arcUrl, ct);
                var best = arcResponse?.Candidates?.FirstOrDefault();

                if (best?.Location != null)
                {
                    return new CoordinatesResult(
                        best.Location.Y,
                        best.Location.X,
                        string.IsNullOrWhiteSpace(best.Address) ? trimmed : best.Address);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        [Description("מחשב מרחק אווירי בקילומטרים בין שתי נקודות GPS (קו אורך ורוחב).")]
        public double CalculateDistanceKm(
            [Description("קו רוחב נקודה 1")] double lat1,
            [Description("קו אורך נקודה 1")] double lon1,
            [Description("קו רוחב נקודה 2")] double lat2,
            [Description("קו אורך נקודה 2")] double lon2)
        {
            var r = 6371;
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