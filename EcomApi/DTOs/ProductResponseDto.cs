using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApi.DTOs
{
    public class ProductResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; } // Include related data
    }
}