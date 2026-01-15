using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EcommerceApi.Models;

namespace EcommerceApi.DTOs
{
    public class OrderDto
    {
        [Required(ErrorMessage = "Order items are required")]
        [MinLength(1, ErrorMessage = "Order must have at least one item")]
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Shipping address must be between 10 and 500 characters")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}