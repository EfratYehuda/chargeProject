using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.USERDTO
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Access { get; set; } = string.Empty;
        public int? DriverId { get; set; }   // קיים אם המשתמש נהג
        public int? OwnerId { get; set; }    // קיים אם המשתמש בעל עמדות
    }
}
