using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceApi.DTOs;
using EcommerceApi.Enums;
using EcommerceApi.Helpers;
using EcommerceApi.Interfaces;

namespace EcommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetOrders()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var orders = await _orderService.GetAllOrdersAsync(userId, userRole);

            return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderById(long id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var order = await _orderService.GetOrderByIdAsync(id, userId, userRole);

            if (order == null)
            {
                return NotFound(ApiResponse.FailureResponse("Order not found or you don't have permission to view it"));
            }

            return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order retrieved successfully"));
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var userId = GetCurrentUserId();

            var (success, message, order) = await _orderService.CreateOrderAsync(dto, userId);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order!.Id },
                ApiResponse<OrderResponseDto>.SuccessResponse(order, message)
            );
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var (success, message) = await _orderService.UpdateOrderStatusAsync(id, dto.Status, userId, userRole);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            return Ok(ApiResponse.SuccessResponse(message));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(long id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var (success, message) = await _orderService.DeleteOrderAsync(id, userId, userRole);

            if (!success)
            {
                return BadRequest(ApiResponse.FailureResponse(message));
            }

            return Ok(ApiResponse.SuccessResponse(message));
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.User;
        }
    }
}