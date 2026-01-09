
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcomApi.Enums;


namespace EcomApi.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
