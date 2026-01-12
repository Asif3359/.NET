using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs
{
    public class CategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}