using EcommerceApi.DTOs;
using EcommerceApi.Enums;
using EcommerceApi.Interfaces;
using EcommerceApi.Models;

namespace EcommerceApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(long? userId = null, UserRole? userRole = null)
        {
            IEnumerable<Order> orders;

            // Admin can see all orders, User can only see their own
            if (userRole == UserRole.Admin)
            {
                orders = await _unitOfWork.Orders.GetOrdersWithDetailsAsync();
            }
            else if (userId.HasValue)
            {
                orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId.Value);
            }
            else
            {
                return Enumerable.Empty<OrderResponseDto>();
            }

            return orders.Select(o => MapToOrderResponseDto(o));
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(long id, long? userId = null, UserRole? userRole = null)
        {
            var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(id);

            if (order == null)
                return null;

            // Check authorization: Admin can see all, User can only see their own
            if (userRole != UserRole.Admin && order.UserId != userId)
            {
                return null;
            }

            return MapToOrderResponseDto(order);
        }

        public async Task<(bool Success, string Message, OrderResponseDto? Order)> CreateOrderAsync(OrderDto dto, long userId)
        {
            try
            {
                // Validate user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                // Validate all products exist and calculate total
                var orderItems = new List<OrderItem>();
                double totalAmount = 0;

                foreach (var itemDto in dto.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        return (false, $"Product with ID {itemDto.ProductId} not found", null);
                    }

                    if (itemDto.Quantity <= 0)
                    {
                        return (false, $"Invalid quantity for product {product.Name}", null);
                    }

                    totalAmount += product.Price * itemDto.Quantity;
                }

                // Create order with items
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    ShippingAddress = dto.ShippingAddress,
                    OrderItems = new List<OrderItem>()
                };

                // Create order items
                foreach (var itemDto in dto.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    
                    var orderItem = new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        Price = product!.Price
                    };

                    order.OrderItems.Add(orderItem);
                }

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Reload order with details
                var createdOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.Id);

                _logger.LogInformation("Order {OrderNumber} created for user {UserId}", order.OrderNumber, userId);

                return (true, "Order created successfully", MapToOrderResponseDto(createdOrder!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(long id, string status, long? userId = null, UserRole? userRole = null)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return (false, "Order not found");
                }

                // Only admin can update order status
                if (userRole != UserRole.Admin)
                {
                    return (false, "Unauthorized to update order status");
                }

                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    return (false, "Invalid order status");
                }

                order.Status = status;

                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);

                return (true, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeleteOrderAsync(long id, long? userId = null, UserRole? userRole = null)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                {
                    return (false, "Order not found");
                }

                // Users can only delete their own orders if status is Pending
                // Admin can delete any order
                if (userRole != UserRole.Admin)
                {
                    if (order.UserId != userId)
                    {
                        return (false, "Unauthorized to delete this order");
                    }

                    if (order.Status != "Pending")
                    {
                        return (false, "Can only delete pending orders");
                    }
                }

                await _unitOfWork.Orders.DeleteAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} deleted", id);

                return (true, "Order deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                throw;
            }
        }

        private OrderResponseDto MapToOrderResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                UserId = order.UserId,
                UserName = order.User?.Name,
                UserEmail = order.User?.Email,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown",
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Subtotal = oi.Price * oi.Quantity
                }).ToList(),
                TotalAmount = order.OrderItems.Sum(oi => oi.Price * oi.Quantity)
            };
        }
    }
}
