
using System.ComponentModel.DataAnnotations;

namespace q03.Models
{
    public class OrderDto
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public long ProductId { get; set; }
    }
}