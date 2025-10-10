using CateringManagement.Managers;
using CateringManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using System.Security.Claims;

namespace CateringManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderManager _orderManager;

        public OrderController(IOrderManager orderManager)
        {
            _orderManager = orderManager;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                if (request == null || request.Items == null || !request.Items.Any())
                    return BadRequest(new { Message = "Invalid order data.", CorrelationId = correlationId });

                try
                {
                    // Extract UserId from JWT token claims
                    var userIdClaim = User.FindFirst("UserId");
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        return Unauthorized(new { Message = "Invalid user token.", CorrelationId = correlationId });
                    }

                    var order = new Order
                    {
                        UserId = userId,  // Use ID from JWT
                        EventDate = request.EventDate.Date,
                        EventTime = request.EventTime,
                        Venue = request.Venue,
                        ContactNumber = request.ContactNumber,
                        Notes = request.Notes
                    };

                    var items = request.Items.Select(i => new OrderItem
                    {
                        MenuId = i.MenuId,
                        Quantity = i.Quantity
                    }).ToList();

                    decimal totalAmount = await _orderManager.CreateOrderAsync(order, items);

                    return Ok(new
                    {
                        Message = "Order created successfully",
                        TotalAmount = totalAmount,
                        CorrelationId = correlationId
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Internal server error while creating order.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetOrdersSummary()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                    int? userId = null;

                    if (roleClaim != null && roleClaim.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                    {
                        var userIdClaim = User.FindFirst("UserId");
                        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int id))
                        {
                            return Unauthorized(new { Message = "Invalid user token.", CorrelationId = correlationId });
                        }
                        userId = id; // Only fetch orders for this user
                    }

                    var orders = await _orderManager.GetOrdersSummaryAsync(userId);

                    return Ok(new
                    {
                        Data = orders,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Error retrieving order summary.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var order = await _orderManager.GetOrderByIdAsync(orderId);

                    if (order == null)
                        return NotFound(new { Message = "Order not found", CorrelationId = correlationId });

                    // If user is Customer, ensure they can access only their own order
                    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                    if (roleClaim != null && roleClaim.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                    {
                        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                            return Unauthorized(new { Message = "Invalid user token.", CorrelationId = correlationId });

                        if (order.UserId != userId)
                            return StatusCode(403, new { Message = "Access denied to this order.", CorrelationId = correlationId });

                    }

                    return Ok(new { Data = order, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Error fetching order.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromQuery] string orderStatus)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                if (string.IsNullOrWhiteSpace(orderStatus))
                    return BadRequest(new { Message = "Order status cannot be empty.", CorrelationId = correlationId });

                try
                {
                    await _orderManager.UpdateOrderStatusAsync(orderId, orderStatus);

                    return Ok(new
                    {
                        Message = "Order status updated successfully.",
                        OrderId = orderId,
                        NewStatus = orderStatus,
                        CorrelationId = correlationId
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return NotFound(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while updating order status.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }
    }
}
