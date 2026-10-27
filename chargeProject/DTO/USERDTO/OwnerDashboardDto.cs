using DTO.REVIEWDTO;
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.USERDTO
{
    /// <summary>לוח הבקרה של בעל העמדה: עמדות פעילות, ביקורות ותקלות בזמן אמת</summary>
    public class OwnerDashboardDto
    {
        public int OwnerId { get; set; }
        public int TotalStationsCount { get; set; }
        public int ActiveStationsCount { get; set; }
        public int FaultyStationsCount { get; set; }
        public int TotalBookings { get; set; }
        public double? AverageRating { get; set; }
        public List<StationDto> Stations { get; set; } = new();
        public List<ReviewDto> RecentReviews { get; set; } = new();
        public List<StationStatusLogDto> RecentFaultReports { get; set; } = new();
    }
}
