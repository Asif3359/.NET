using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApi.DTOs
{
    public class ProductResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}