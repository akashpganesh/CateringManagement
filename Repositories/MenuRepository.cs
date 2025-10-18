using CateringManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CateringManagement.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MenuRepository> _logger;

        public MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<int> CreateMenuItemAsync(MenuItem menu)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Name", menu.Name);
                    parameters.Add("@Category", menu.Category);
                    parameters.Add("@Price", menu.Price);
                    parameters.Add("@Availability", menu.Availability);
                    parameters.Add("@IsVegetarian", menu.IsVegetarian);

                    return await db.QuerySingleAsync<int>(
                        "sp_CreateMenuItem",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while creating menu item: {MenuName}", menu.Name);
                    throw;
                }
            }
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    return await db.QueryAsync<MenuItem>(
                        "sp_GetAllMenuItems",
                        commandType: CommandType.StoredProcedure
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while fetching all menu items");
                    throw;
                }
            }
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(int menuId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@MenuId", menuId);

                    return await db.QuerySingleAsync<MenuItem>(
                        "sp_GetMenuItemById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
                catch (SqlException ex) when (ex.Class == 16 && ex.Message.Contains("Menu item not found"))
                {
                    _logger.LogWarning("Menu item not found: ID {MenuId}", menuId);
                    throw new InvalidOperationException("Menu item not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while fetching menu item by ID {MenuId}", menuId);
                    throw;
                }
            }
        }

        public async Task UpdateMenuItemAsync(int menuId, UpdateMenuItem menu)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MenuId", menuId);
                parameters.Add("@Name", menu.Name);
                parameters.Add("@Category", menu.Category);
                parameters.Add("@Price", menu.Price);
                parameters.Add("@Availability", menu.Availability);
                parameters.Add("@IsVegetarian", menu.IsVegetarian);

                try
                {
                    await db.ExecuteAsync("sp_PartialUpdateMenuItem", parameters, commandType: CommandType.StoredProcedure);
                }
                catch (SqlException ex) when (ex.Class == 16 && ex.Message.Contains("Menu item not found"))
                {
                    _logger.LogWarning("Menu item not found during update: ID {MenuId}", menuId);
                    throw new InvalidOperationException("Menu item not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while updating menu item ID {MenuId}", menuId);
                    throw;
                }
            }
        }

        public async Task DeleteMenuItemAsync(int menuId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MenuId", menuId);

                try
                {
                    await db.ExecuteAsync("sp_DeleteMenuItem", parameters, commandType: CommandType.StoredProcedure);
                }
                catch (SqlException ex) when (ex.Class == 16 && ex.Message.Contains("Menu item not found"))
                {
                    _logger.LogWarning("Menu item not found during delete: ID {MenuId}", menuId);
                    throw new InvalidOperationException("Menu item not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while deleting menu item ID {MenuId}", menuId);
                    throw;
                }
            }
        }
    }
}