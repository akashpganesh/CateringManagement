using CateringManagement.Models;

namespace CateringManagement.Repositories
{
    public interface IOrderRepository
    {
        Task<decimal> CreateOrderAsync(Order order, List<OrderItem> items);
        Task<IEnumerable<OrderSummary>> GetOrdersSummaryAsync(int? userId = null);
        Task<OrderDetails> GetOrderByIdAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, string orderStatus);
    }
}
