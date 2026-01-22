using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApi.Models
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}