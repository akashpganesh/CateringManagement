using CateringManagement.Managers;
using CateringManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace CateringManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuManager _menuManager;

        public MenuController(IMenuManager menuManager)
        {
            _menuManager = menuManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItem menu)
        {
            // Generate or get CorrelationId
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                if (menu == null)
                    return BadRequest(new { Message = "Menu item cannot be null.", CorrelationId = correlationId });

                try
                {
                    var createdMenu = await _menuManager.CreateMenuItemAsync(menu);
                    return Ok(new
                    {
                        Message = "Menu item created successfully",
                        Data = createdMenu,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Internal server error while creating menu item.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllMenuItems()
        {
            // Generate or get CorrelationId
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var menuItems = await _menuManager.GetAllMenuItemsAsync();
                    return Ok(new
                    {
                        Message = "Menu items retrieved successfully",
                        Data = menuItems,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Internal server error while retrieving menu items.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMenuItemById(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var menuItem = await _menuManager.GetMenuItemByIdAsync(id);
                    return Ok(new
                    {
                        Message = "Menu item retrieved successfully",
                        Data = menuItem,
                        CorrelationId = correlationId
                    });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
                {
                    return NotFound(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Internal server error while retrieving menu item.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] UpdateMenuItem menu)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                if (menu == null)
                    return BadRequest(new { Message = "Invalid menu item data.", CorrelationId = correlationId });

                try
                {
                    await _menuManager.UpdateMenuItemAsync(id, menu);
                    return Ok(new
                    {
                        Message = "Menu item updated successfully (partial update)",
                        CorrelationId = correlationId
                    });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
                {
                    return NotFound(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "Internal server error while updating menu item.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await _menuManager.DeleteMenuItemAsync(id);
                    return Ok(new
                    {
                        Message = "Menu item deleted successfully",
                        CorrelationId = correlationId
                    });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
                {
                    return NotFound(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while deleting the menu item.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }
    }
}
