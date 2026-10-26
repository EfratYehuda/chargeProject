using Microsoft.AspNetCore.Mvc;
using BLL.Services;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationStatusLogsController : ControllerBase
    {
        private readonly IStationStatusLogsService _stationStatusLogsService;
        public StationStatusLogsController(IStationStatusLogsService _stationStatusLogsService)
        {
            _stationStatusLogsService = _stationStatusLogsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            try { return Ok(await _stationStatusLogsService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogById(int id)
        {
            try
            {
                var log = await _stationStatusLogsService.GetByIdAsync(id);
                if (log == null) return NotFound($"Log with ID {id} not found.");
                return Ok(log);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            try { return Ok(await _stationStatusLogsService.GetByStationIdAsync(stationId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("station/{stationId}/latest")]
        public async Task<IActionResult> GetLatestByStationId(int stationId)
        {
            try
            {
                var log = await _stationStatusLogsService.GetLatestByStationIdAsync(stationId);
                if (log == null) return NotFound($"Latest log for station ID {stationId} not found.");
                return Ok(log);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            try
            {
                var success = await _stationStatusLogsService.DeleteAsync(id);
                if (!success) return NotFound($"Log with ID {id} not found.");
                return Ok($"Log with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}