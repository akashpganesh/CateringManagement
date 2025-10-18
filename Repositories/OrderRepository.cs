using CateringManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CateringManagement.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(IConfiguration config, ILogger<OrderRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
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
                    _logger.LogWarning(ex, "Attempt to create duplicate order for user {UserId}", order.UserId);
                    throw new InvalidOperationException(ex.Message);
                }
                catch (SqlException ex)
                {
                    _logger.LogError(ex, "SQL error while creating order for user {UserId}", order.UserId);
                    throw new Exception("An error occurred while creating the order.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while creating order for user {UserId}", order.UserId);
                    throw;
                }
            }
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersSummaryAsync(int? userId = null)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var orders = await db.QueryAsync<OrderSummary>(
                        "sp_GetOrdersSummary",
                        new { UserId = userId },
                        commandType: CommandType.StoredProcedure
                    );

                    return orders;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching order summaries for user {UserId}", userId);
                    throw;
                }
            }
        }

        public async Task<OrderDetails> GetOrderByIdAsync(int orderId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching order details for OrderId {OrderId}", orderId);
                    throw;
                }
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
                    _logger.LogWarning(ex, "Attempt to update non-existing order {OrderId}", orderId);
                    throw new InvalidOperationException("Order not found.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating order status for OrderId {OrderId}", orderId);
                    throw;
                }
            }
        }

        public async Task CancelOrderAsync(int orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_CancelOrder", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@OrderId", orderId);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(ex, "Attempt to cancel non-existing order {OrderId}", orderId);
                        throw new InvalidOperationException("Order not found.");
                    }

                    if (ex.Message.Contains("already Cancelled", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(ex, "Attempt to cancel already cancelled order {OrderId}", orderId);
                        throw new InvalidOperationException("Cannot cancel an order that is already cancelled.");
                    }

                    if (ex.Message.Contains("at least 4 days", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(ex, "Attempt to cancel order {OrderId} less than 4 days before event", orderId);
                        throw new InvalidOperationException("Order can only be cancelled at least 4 days before the event date.");
                    }

                    _logger.LogError(ex, "SQL error while cancelling order {OrderId}", orderId);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while cancelling order {OrderId}", orderId);
                    throw;
                }
            }
        }
    }
}