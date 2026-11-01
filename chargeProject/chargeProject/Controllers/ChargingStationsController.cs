using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO.STATIONDTO;
using System.Security.Claims;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChargingStationsController : ControllerBase
    {
        private readonly IChargingStationsService _chargingStationsService;

        public ChargingStationsController(IChargingStationsService chargingStationsService)
        {
            _chargingStationsService = chargingStationsService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            try { return Ok(await _chargingStationsService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(int id)
        {
            try
            {
                var station = await _chargingStationsService.GetByIdAsync(id);
                if (station == null) return NotFound($"Station with ID {id} not found.");
                return Ok(station);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetStationsByOwner(int ownerId)
        {
            try { return Ok(await _chargingStationsService.GetByOwnerIdAsync(ownerId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("radius")]
        public async Task<IActionResult> GetActiveInRadius([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius, [FromQuery] string? chargerType)
        {
            try { return Ok(await _chargingStationsService.GetActiveInRadiusAsync(lat, lng, radius, chargerType)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] CreateUpdateStationDto dto)
        {
            try
            {
                return Ok(await _chargingStationsService.AddAsync(dto));
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStation(int id, [FromBody] CreateUpdateStationDto dto)
        {
            try
            {
                // שליפה ישירה של מזהה המשתמש מתוך הטוקן
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? ownerId = await _chargingStationsService.GetOwnerIdByUserIdAsync(userId);
                if (ownerId == null) return Unauthorized("User is not a station owner.");

                var updated = await _chargingStationsService.UpdateAsync(id, ownerId.Value, dto);
                if (updated == null) return NotFound($"Station with ID {id} not found or does not belong to you.");

                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? ownerId = await _chargingStationsService.GetOwnerIdByUserIdAsync(userId);
                if (ownerId == null) return Unauthorized("User is not a station owner.");

                var updated = await _chargingStationsService.SetActiveAsync(id, ownerId.Value, isActive);
                if (updated == null) return NotFound($"Station with ID {id} not found or does not belong to you.");

                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? ownerId = await _chargingStationsService.GetOwnerIdByUserIdAsync(userId);
                if (ownerId == null) return Unauthorized("User is not a station owner.");

                var success = await _chargingStationsService.DeleteAsync(id, ownerId.Value);
                if (!success) return NotFound($"Station with ID {id} not found or does not belong to you.");

                return Ok($"Station with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}