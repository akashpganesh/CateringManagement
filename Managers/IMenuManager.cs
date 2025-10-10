using CateringManagement.Models;

namespace CateringManagement.Managers
{
    public interface IMenuManager
    {
        Task<MenuItem> CreateMenuItemAsync(MenuItem menu);
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
        Task<MenuItem> GetMenuItemByIdAsync(int menuId);
        Task UpdateMenuItemAsync(int menuId, UpdateMenuItem menu);
        Task DeleteMenuItemAsync(int menuId);
    }
}
