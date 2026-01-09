using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace EcomApi.DTOs
{
    public class SignupDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; }= string.Empty;


        [Required, MinLength(6)]
        public string Password { get; set; }= string.Empty;

    }
}