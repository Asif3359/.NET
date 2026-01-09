using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApi.DTOs
{
    public class OrderItemResponseDto
    {
        public long OrderId { get; set; } // Add this
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; } // Change from computed to settable
    }
}