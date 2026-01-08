namespace q03.Models;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

