using CateringManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringManagement.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<decimal> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var tvp = new DataTable();
                tvp.Columns.Add("MenuId", typeof(int));
                tvp.Columns.Add("Quantity", typeof(int));

                foreach (var item in items)
                    tvp.Rows.Add(item.MenuId, item.Quantity);

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", order.UserId);
                parameters.Add("@EventDate", order.EventDate.Date);
                parameters.Add("@EventTime", order.EventTime);
                parameters.Add("@Venue", order.Venue);
                parameters.Add("@ContactNumber", order.ContactNumber);
                parameters.Add("@Notes", order.Notes);
                parameters.Add("@OrderItems", tvp.AsTableValuedParameter("TVP_OrderItems"));
                parameters.Add("@TotalAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output);

                try
                {
                    await db.ExecuteAsync("sp_CreateOrder", parameters, commandType: CommandType.StoredProcedure);
                    return parameters.Get<decimal>("@TotalAmount"); // Return total amount
                }
                catch (SqlException ex) when (ex.Class == 16 && ex.Message.Contains("already an order"))
                {
                    throw new InvalidOperationException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersSummaryAsync(int? userId = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var orders = await db.QueryAsync<OrderSummary>(
                    "sp_GetOrdersSummary",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure
                );

                return orders;
            }
        }

        public async Task<OrderDetails> GetOrderByIdAsync(int orderId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var orderDict = new Dictionary<int, OrderDetails>();

                var result = await db.QueryAsync<OrderDetails, OrderItemDetails, OrderDetails>(
                    "sp_GetOrderById",
                    (order, item) =>
                    {
                        if (!orderDict.TryGetValue(order.OrderId, out var currentOrder))
                        {
                            currentOrder = order;
                            currentOrder.Items = new List<OrderItemDetails>();
                            orderDict.Add(order.OrderId, currentOrder);
                        }

                        currentOrder.Items.Add(item);
                        return currentOrder;
                    },
                    param: new { OrderId = orderId },
                    splitOn: "MenuId",
                    commandType: CommandType.StoredProcedure
                );

                return orderDict.Values.FirstOrDefault(); // Return single order
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@OrderId", orderId);
                parameters.Add("@OrderStatus", orderStatus);

                try
                {
                    await db.ExecuteAsync("sp_UpdateOrderStatus", parameters, commandType: CommandType.StoredProcedure);
                }
                catch (SqlException ex) when (ex.Message.Contains("Order not found"))
                {
                    throw new InvalidOperationException("Order not found.");
                }
            }
        }

    }
}
