using CateringManagement.Models;

namespace CateringManagement.Managers
{
    public interface IOrderManager
    {
        Task<decimal> CreateOrderAsync(Order order, List<OrderItem> items);
        Task<IEnumerable<OrderSummary>> GetOrdersSummaryAsync(int? userId = null);
        Task<OrderDetails> GetOrderByIdAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, string orderStatus);
        Task CancelOrderAsync(int orderId);
    }
}
