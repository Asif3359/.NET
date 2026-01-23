using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApi.DTOs
{
    public class CommentDto
    {
        [Required]
        [StringLength(500, MinimumLength = 2)]
        public string Content { get; set; } = string.Empty;
    }
}