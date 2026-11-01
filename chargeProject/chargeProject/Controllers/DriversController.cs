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
    public class DriversController : ControllerBase
    {
        private readonly IDriversService _driversService;

        public DriversController(IDriversService driversService)
        {
            _driversService = driversService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            try { return Ok(await _driversService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            try
            {
                var driver = await _driversService.GetByIdAsync(id);
                if (driver == null) return NotFound($"Driver with ID {id} not found.");
                return Ok(driver);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDriverByUserId(int userId)
        {
            try
            {
                var driver = await _driversService.GetByUserIdAsync(userId);
                if (driver == null) return NotFound($"Driver with User ID {userId} not found.");
                return Ok(driver);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try { return Ok(await _driversService.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                // וידוא IDOR: הנהג יכול למחוק רק את הפרופיל ששייך אליו
                var driverByUser = await _driversService.GetByUserIdAsync(userId);
                if (driverByUser == null || driverByUser.Id != id)
                {
                    return Forbid();
                }

                var success = await _driversService.DeleteAsync(id);
                if (!success) return NotFound($"Driver with ID {id} not found.");
                return Ok($"Driver with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}