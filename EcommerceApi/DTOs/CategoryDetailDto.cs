using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApi.DTOs
{
    public class CategoryDetailDto
    {

        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public List<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();
    }
}