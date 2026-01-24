using System.ComponentModel.DataAnnotations;
using BlogApi.Enums;

namespace BlogApi.DTOs
{
    public class UpdatePostDto
    {
        [StringLength(200, MinimumLength = 5)]
        public string? Title { get; set; }

        [StringLength(1000, MinimumLength = 10)]
        public string? Content { get; set; }

        public PostStatus? Status { get; set; }

        public List<string>? Tags { get; set; }
    }
}