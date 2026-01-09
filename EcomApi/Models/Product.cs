using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApi.Models
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public double Price { get; set; } = 0;
        public string Description { get; set; }= string.Empty;

        public long? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
