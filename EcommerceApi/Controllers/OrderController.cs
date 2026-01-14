using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            var currentUserid = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == currentUserid);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate) // Show newest first
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                    User = new UserInfoDto
                    {
                        Id = o.User.Id,
                        Name = o.User.Name,
                        Email = o.User.Email
                    },
                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity
                    }).ToList()
                }).ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(long id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not dound" });
            }

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            if (order.UserId != userId && !isAdmin)
            {
                return Forbid();
            }
            var orderDto = new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                User = new UserInfoDto
                {
                    Id = order.User.Id,
                    Name = order.User.Name,
                    Email = order.User.Email
                },
                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.UnitPrice * oi.Quantity
                }).ToList()
            };
            return Ok(orderDto);
        }

        [HttpPost("")]
        public async Task<ActionResult<OrderResponseDto>> PostOrder([FromBody] OrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { message = "Order must contain at least one item" });
            }

            var orderItems = new List<OrderItem>();
            double totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Product with ID {itemDto.ProductId} not found" });
                }

                if (itemDto.Quantity <= 0)
                {
                    return BadRequest(new { message = $"Quantity must be greater than 0 for product: {product.Name}" });
                }

                if (itemDto.Quantity > 50)
                {
                    return BadRequest(new { message = $"Maximum quantity is 100 for product: {product.Name}" });
                }

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                };

                orderItems.Add(orderItem);
                totalAmount += product.Price * itemDto.Quantity;

            }

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                UserId = userId,
                OrderItems = orderItems
            };
            _context.Orders.Add(order);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Failed to create order", error = ex.Message });
            }

            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (createdOrder == null)
            {
                return StatusCode(500, new { message = "Order was created but could not be retrieved" });
            }

            var responseDto = new OrderResponseDto
            {
                Id = createdOrder.Id,
                OrderNumber = createdOrder.OrderNumber,
                OrderDate = createdOrder.OrderDate,
                Status = createdOrder.Status,
                TotalAmount = totalAmount,

                User = new UserInfoDto
                {
                    Id = createdOrder.User.Id,
                    Name = createdOrder.User.Name,
                    Email = createdOrder.User.Email
                },
                Items = createdOrder.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.UnitPrice * oi.Quantity
                }).ToList()

            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, responseDto);
        }

        // PATCH: api/orders/5/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Validate status
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(statusDto.Status))
                return BadRequest(new { message = "Invalid order status" });

            // Business logic: Validate status transitions
            if (order.Status == "Delivered" && statusDto.Status != "Delivered")
                return BadRequest(new { message = "Cannot change status of a delivered order" });

            if (order.Status == "Cancelled" && statusDto.Status != "Cancelled")
                return BadRequest(new { message = "Cannot change status of a cancelled order" });

            order.Status = statusDto.Status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                    return NotFound(new { message = "Order not found during update" });
                throw;
            }

            return Ok(new
            {
                message = $"Order status updated to {statusDto.Status}",
                orderId = id,
                newStatus = statusDto.Status
            });
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(long id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");


            if (order.UserId != userId && !isAdmin)
                return Forbid();


            if (order.Status != "Pending")
            {
                var allowedStatuses = isAdmin ? new[] { "Pending", "Processing" } : new[] { "Pending" };

                if (!allowedStatuses.Contains(order.Status))
                {
                    return BadRequest(new
                    {
                        message = isAdmin
                            ? "Only pending or processing orders can be cancelled"
                            : "Only pending orders can be cancelled by users"
                    });
                }
            }

            order.Status = "Cancelled";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                    return NotFound(new { message = "Order not found during cancellation" });
                throw;
            }

            return NoContent();
        }


        // GET: api/orders/user/5
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrdersByUser(long userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

            if (!userExists)
                return NotFound(new { message = "User not found" });

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                    User = new UserInfoDto
                    {
                        Id = o.User.Id,
                        Name = o.User.Name,
                        Email = o.User.Email
                    },
                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity
                    }).ToList()
                }).ToListAsync();

            return Ok(orders);
        }

        // GET: api/orders/status/pending 
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrdersByStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            if (!validStatuses.Contains(status))
                return BadRequest(new { message = "Invalid order status" });

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
                    User = new UserInfoDto
                    {
                        Id = o.User.Id,
                        Name = o.User.Name,
                        Email = o.User.Email
                    },
                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity
                    }).ToList()
                }).ToListAsync();

            return Ok(orders);
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User id is not found in claims");
            }

            return long.Parse(userIdClaim);
        }
        private bool OrderExists(long id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        private string GenerateOrderNumber()
        {
            var random = new Random();
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{random.Next(1000, 9999)}";
        }
    }
}