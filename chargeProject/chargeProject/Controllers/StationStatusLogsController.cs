using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.IRepositories;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationStatusLogsController : ControllerBase
    {
        private readonly InterfaceStationStatusLogs _logsRepository;

        public StationStatusLogsController(InterfaceStationStatusLogs logsRepository)
        {
            _logsRepository = logsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            try { return Ok(await _logsRepository.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogById(int id)
        {
            try
            {
                var log = await _logsRepository.GetByIdAsync(id);
                if (log == null) return NotFound($"Log with ID {id} not found.");
                return Ok(log);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            try { return Ok(await _logsRepository.GetByStationIdAsync(stationId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("station/{stationId}/latest")]
        public async Task<IActionResult> GetLatestByStationId(int stationId)
        {
            try
            {
                var log = await _logsRepository.GetLatestByStationIdAsync(stationId);
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
                var success = await _logsRepository.DeleteAsync(id);
                if (!success) return NotFound($"Log with ID {id} not found.");
                return Ok($"Log with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}