using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.USERDTO
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
    }
}
