using CateringManagement.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringManagement.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateMenuItemAsync(MenuItem menu)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Name", menu.Name);
                parameters.Add("@Category", menu.Category);
                parameters.Add("@Price", menu.Price);
                parameters.Add("@Availability", menu.Availability);
                parameters.Add("@IsVegetarian", menu.IsVegetarian);

                return await db.QuerySingleAsync<int>("sp_CreateMenuItem", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryAsync<MenuItem>("sp_GetAllMenuItems", commandType: CommandType.StoredProcedure);
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

                    var menuItem = await db.QuerySingleAsync<MenuItem>(
                        "sp_GetMenuItemById",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                    return menuItem;
                }
                catch (SqlException ex) when (ex.Class == 16 && ex.Message.Contains("Menu item not found"))
                {
                    throw new InvalidOperationException("Menu item not found");
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
                    throw new InvalidOperationException("Menu item not found");
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
                    throw new InvalidOperationException("Menu item not found");
                }
            }
        }
    }
}
