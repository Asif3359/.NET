using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApi.DTOs
{
    public class CategoryDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();
    }
}