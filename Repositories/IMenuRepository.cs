using CateringManagement.Models;

namespace CateringManagement.Repositories
{
    public interface IMenuRepository
    {
        Task<int> CreateMenuItemAsync(MenuItem menu);
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
        Task<MenuItem> GetMenuItemByIdAsync(int menuId);
        Task UpdateMenuItemAsync(int menuId, UpdateMenuItem menu);
        Task DeleteMenuItemAsync(int menuId);
    }
}
