using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.STATIONDTO
{
    /// <summary>עדכון שעות השבתה אוטומטיות (0-23) של עמדה</summary>
    public class UpdateBlockedHoursDto
    {
        public List<int> BlockedHours { get; set; } = new();
    }
}
