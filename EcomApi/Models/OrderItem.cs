using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApi.Models
{
    public class OrderItem
    {
        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public long ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
    }

}
