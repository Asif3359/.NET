using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApi.DTOs
{
    public class TagDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}