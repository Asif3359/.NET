using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EcommerceApi.Models;

namespace EcommerceApi.DTOs
{
    public class OrderItemDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}