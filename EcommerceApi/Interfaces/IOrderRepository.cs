using EcommerceApi.Models;

namespace EcommerceApi.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(long userId);
        Task<Order?> GetOrderWithDetailsAsync(long id);
        Task<IEnumerable<Order>> GetOrdersWithDetailsAsync();
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
    }
}
