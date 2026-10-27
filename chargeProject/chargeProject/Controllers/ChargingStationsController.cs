using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO.STATIONDTO;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingStationsController : ControllerBase
    {
        private readonly IChargingStationsService _chargingStationsService;

        public ChargingStationsController(IChargingStationsService chargingStationsService)
        {
            _chargingStationsService = chargingStationsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            try { return Ok(await _chargingStationsService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

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

        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetStationsByOwner(int ownerId)
        {
            try { return Ok(await _chargingStationsService.GetByOwnerIdAsync(ownerId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("radius")]
        public async Task<IActionResult> GetActiveInRadius([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius, [FromQuery] string? chargerType)
        {
            try { return Ok(await _chargingStationsService.GetActiveInRadiusAsync(lat, lng, radius, chargerType)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStation([FromBody] CreateUpdateStationDto dto)
        {
            try { return Ok(await _chargingStationsService.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStation(int id, [FromBody] CreateUpdateStationDto dto)
        {
            try
            {
                var updated = await _chargingStationsService.UpdateAsync(id, dto);
                if (updated == null) return NotFound($"Station with ID {id} not found.");
                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive)
        {
            try
            {
                var updated = await _chargingStationsService.SetActiveAsync(id, isActive);
                if (updated == null) return NotFound($"Station with ID {id} not found.");
                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            try
            {
                var success = await _chargingStationsService.DeleteAsync(id);
                if (!success) return NotFound($"Station with ID {id} not found.");
                return Ok($"Station with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}