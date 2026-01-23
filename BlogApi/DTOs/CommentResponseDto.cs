using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApi.DTOs
{
    public class CommentResponseDto
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

    }
}