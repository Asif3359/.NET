using EcommerceApi.DTOs;
using EcommerceApi.Enums;

namespace EcommerceApi.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(long? userId = null, UserRole? userRole = null);
        Task<OrderResponseDto?> GetOrderByIdAsync(long id, long? userId = null, UserRole? userRole = null);
        Task<(bool Success, string Message, OrderResponseDto? Order)> CreateOrderAsync(OrderDto dto, long userId);
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(long id, string status, long? userId = null, UserRole? userRole = null);
        Task<(bool Success, string Message)> DeleteOrderAsync(long id, long? userId = null, UserRole? userRole = null);
    }
}
