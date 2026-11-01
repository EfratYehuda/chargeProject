using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO.USERDTO;
using System.Security.Claims;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnersService _ownersService;

        public OwnerController(IOwnersService ownersService)
        {
            _ownersService = ownersService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllOwners()
        {
            try { return Ok(await _ownersService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnerById(int id)
        {
            try
            {
                var owner = await _ownersService.GetByIdAsync(id);
                if (owner == null) return NotFound($"Owner with ID {id} not found.");
                return Ok(owner);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOwnerByUserId(int userId)
        {
            try
            {
                var owner = await _ownersService.GetByUserIdAsync(userId);
                if (owner == null) return NotFound($"Owner with User ID {userId} not found.");
                return Ok(owner);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        // מאובטח IDOR: צפייה בדשבורד רק של בעל העמדה המחובר
        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetDashboard(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                var ownerByUser = await _ownersService.GetByUserIdAsync(userId);
                if (ownerByUser == null || ownerByUser.Id != id)
                {
                    return Forbid();
                }

                var dashboard = await _ownersService.GetDashboardAsync(id);
                if (dashboard == null) return NotFound($"Dashboard for owner ID {id} not found.");
                return Ok(dashboard);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOwner([FromBody] CreateOwnerDto dto)
        {
            try { return Ok(await _ownersService.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOwner(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                var ownerByUser = await _ownersService.GetByUserIdAsync(userId);
                if (ownerByUser == null || ownerByUser.Id != id)
                {
                    return Forbid();
                }

                var success = await _ownersService.DeleteAsync(id);
                if (!success) return NotFound($"Owner with ID {id} not found.");
                return Ok($"Owner with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}