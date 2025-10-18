using CateringManagement.Models;
using CateringManagement.Repositories;
using Microsoft.Extensions.Logging;

namespace CateringManagement.Managers
{
    public class OrderManager : IOrderManager
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ILogger<OrderManager> _logger;

        public OrderManager(IOrderRepository orderRepo, ILogger<OrderManager> logger)
        {
            _orderRepo = orderRepo;
            _logger = logger;
        }

        public async Task<decimal> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            try
            {
                // Validation: Orders must be placed at least 4 days in advance
                if (order.EventDate.Date < DateTime.Now.Date.AddDays(4))
                    throw new InvalidOperationException("Orders must be placed at least 4 days before the event date.");

                return await _orderRepo.CreateOrderAsync(order, items);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating order for user {UserId}", order.UserId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order for user {UserId}", order.UserId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersSummaryAsync(int? userId = null)
        {
            try
            {
                return await _orderRepo.GetOrdersSummaryAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders summary for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDetails> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return await _orderRepo.GetOrderByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order details for OrderId {OrderId}", orderId);
                throw;
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            try
            {
                await _orderRepo.UpdateOrderStatusAsync(orderId, orderStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for OrderId {OrderId}", orderId);
                throw;
            }
        }

        public async Task CancelOrderAsync(int orderId)
        {
            try
            {
                await _orderRepo.CancelOrderAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order for OrderId {OrderId}", orderId);
                throw;
            }
        }
    }
}