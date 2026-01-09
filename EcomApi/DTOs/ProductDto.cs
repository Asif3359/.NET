using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace EcomApi.DTOs
{
    public class ProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID is required")]
        public long CategoryId { get; set; }
    }
}