using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApi.DTOs
{
    public class OrderResponseDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public string ShippingAddress {get; set;} = string.Empty;
        public UserInfoDto User { get; set; } = new UserInfoDto();
        public List<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
    }
}