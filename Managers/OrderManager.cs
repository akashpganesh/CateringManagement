using CateringManagement.Models;
using CateringManagement.Repositories;

namespace CateringManagement.Managers
{
    public class OrderManager : IOrderManager
    {
        private readonly IOrderRepository _orderRepo;

        public OrderManager(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<decimal> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            return await _orderRepo.CreateOrderAsync(order, items);
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersSummaryAsync(int? userId = null)
        {
            return await _orderRepo.GetOrdersSummaryAsync(userId);
        }

        public async Task<OrderDetails> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepo.GetOrderByIdAsync(orderId);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            await _orderRepo.UpdateOrderStatusAsync(orderId, orderStatus);
        }
    }
}
