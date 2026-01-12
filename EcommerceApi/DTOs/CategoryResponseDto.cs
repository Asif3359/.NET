using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceApi.DTOs
{
    public class CategoryResponseDto
    {

        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }

    }
}