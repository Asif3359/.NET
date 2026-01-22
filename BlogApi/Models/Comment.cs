using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApi.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public long PostId { get; set; }
        public Post Post { get; set; } = null!;

        public long UserId { get; set; }
        public User User { get; set; } = null!;

        
    }
}