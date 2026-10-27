using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.USERDTO
{
    public class OwnerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int StationsCount { get; set; }
    }
}
