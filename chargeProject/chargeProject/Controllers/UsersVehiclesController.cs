using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO.VEHICLESDTO;
using System.Security.Claims;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersVehiclesController : ControllerBase
    {
        private readonly IUsersVehiclesService _usersVehiclesService;

        public UsersVehiclesController(IUsersVehiclesService usersVehiclesService)
        {
            _usersVehiclesService = usersVehiclesService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            try { return Ok(await _usersVehiclesService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _usersVehiclesService.GetByIdAsync(id);
                if (vehicle == null) return NotFound($"Vehicle with ID {id} not found.");
                return Ok(vehicle);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("driver/{driverId}")]
        public async Task<IActionResult> GetByDriverId(int driverId)
        {
            try { return Ok(await _usersVehiclesService.GetByDriverIdAsync(driverId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateUpdateVehicleDto dto)
        {
            try { return Ok(await _usersVehiclesService.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateUpdateVehicleDto dto)
        {
            try
            {
                // שליפת ה-UserId מתוך הטוקן
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                // המרה ל-DriverId
                int? driverId = await _usersVehiclesService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var updated = await _usersVehiclesService.UpdateAsync(id, driverId.Value, dto);
                if (updated == null) return NotFound($"Vehicle with ID {id} not found or does not belong to you.");
                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPatch("{id}/battery")]
        public async Task<IActionResult> UpdateBattery(int id, [FromQuery] double batteryPercent)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? driverId = await _usersVehiclesService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var updated = await _usersVehiclesService.UpdateCurrentBatteryAsync(id, driverId.Value, batteryPercent);
                if (updated == null) return NotFound($"Vehicle with ID {id} not found or does not belong to you.");
                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? driverId = await _usersVehiclesService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var success = await _usersVehiclesService.DeleteAsync(id, driverId.Value);
                if (!success) return NotFound($"Vehicle with ID {id} not found or does not belong to you.");
                return Ok($"Vehicle with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}