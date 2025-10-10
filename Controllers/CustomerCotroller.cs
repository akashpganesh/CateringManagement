using CateringManagement.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringManagement.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IMenuManager _menuManager;

        public CustomerController(IMenuManager menuManager)
        {
            _menuManager = menuManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyProfile()
        {
            return View();
        }

        public async Task<IActionResult> CreateOrder()
        {
            // Fetch all menu items from manager/service
            var menuItems = await _menuManager.GetAllMenuItemsAsync();

            // Pass to view
            return View(menuItems);
        }
    }
}
