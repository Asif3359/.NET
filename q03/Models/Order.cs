namespace q03.Models;

public class Order
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

