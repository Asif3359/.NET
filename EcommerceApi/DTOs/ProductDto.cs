using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs
{
    public class ProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public double Price { get; set; } = 0;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public long? CategoryId { get; set; }

    }
}