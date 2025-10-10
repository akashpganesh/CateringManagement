using CateringManagement.Models;
using CateringManagement.Repositories;

namespace CateringManagement.Managers
{
    public class MenuManager : IMenuManager
    {
        private readonly IMenuRepository _menuRepo;

        public MenuManager(IMenuRepository menuRepo)
        {
            _menuRepo = menuRepo;
        }

        public async Task<MenuItem> CreateMenuItemAsync(MenuItem menu)
        {
            int menuId = await _menuRepo.CreateMenuItemAsync(menu);
            menu.MenuId = menuId;
            return menu;
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            return await _menuRepo.GetAllMenuItemsAsync();
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(int menuId)
        {
            return await _menuRepo.GetMenuItemByIdAsync(menuId);
        }

        public async Task UpdateMenuItemAsync(int menuId, UpdateMenuItem menu)
        {
            await _menuRepo.UpdateMenuItemAsync(menuId, menu);
        }

        public async Task DeleteMenuItemAsync(int menuId)
        {
            await _menuRepo.DeleteMenuItemAsync(menuId);
        }
    }
}
