using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace EcomApi.DTOs
{
    public class OrderDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Order must have at least one item")]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        public string ShippingAddress {get; set;} = string.Empty;
    }
}