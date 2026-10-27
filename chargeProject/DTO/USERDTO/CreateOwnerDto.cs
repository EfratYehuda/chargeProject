using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO.USERDTO
{
    public class CreateOwnerDto
    {
        [Required]
        public int UserId { get; set; }
    }

}
