using CateringManagement.Models;
using CateringManagement.Repositories;
using Microsoft.Extensions.Logging;

namespace CateringManagement.Managers
{
    public class MenuManager : IMenuManager
    {
        private readonly IMenuRepository _menuRepo;
        private readonly ILogger<MenuManager> _logger;

        public MenuManager(IMenuRepository menuRepo, ILogger<MenuManager> logger)
        {
            _menuRepo = menuRepo;
            _logger = logger;
        }

        public async Task<MenuItem> CreateMenuItemAsync(MenuItem menu)
        {
            try
            {
                int menuId = await _menuRepo.CreateMenuItemAsync(menu);
                menu.MenuId = menuId;
                return menu;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating menu item: {MenuName}", menu.Name);
                throw;
            }
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            try
            {
                return await _menuRepo.GetAllMenuItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all menu items");
                throw;
            }
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(int menuId)
        {
            try
            {
                return await _menuRepo.GetMenuItemByIdAsync(menuId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching menu item by ID {MenuId}", menuId);
                throw;
            }
        }

        public async Task UpdateMenuItemAsync(int menuId, UpdateMenuItem menu)
        {
            try
            {
                await _menuRepo.UpdateMenuItemAsync(menuId, menu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating menu item ID {MenuId}", menuId);
                throw;
            }
        }

        public async Task DeleteMenuItemAsync(int menuId)
        {
            try
            {
                await _menuRepo.DeleteMenuItemAsync(menuId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting menu item ID {MenuId}", menuId);
                throw;
            }
        }
    }
}