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
        [Required]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        public string ShippingAddress { get; set; } = string.Empty;

    }
}