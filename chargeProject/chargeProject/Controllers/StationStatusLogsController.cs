using BLL.Services;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // The status of one station is readable by any logged-in user (drivers need it).
    // Listing every log and deleting logs is admin only.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StationStatusLogsController : ControllerBase
    {
        private readonly IStationStatusLogsService _stationStatusLogsService;

        public StationStatusLogsController(IStationStatusLogsService stationStatusLogsService)
        {
            _stationStatusLogsService = stationStatusLogsService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllLogs()
        {
            return Ok(await _stationStatusLogsService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetLogById(int id)
        {
            return Ok(await _stationStatusLogsService.GetByIdAsync(id));
        }

        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            return Ok(await _stationStatusLogsService.GetByStationIdAsync(stationId));
        }

        [HttpGet("station/{stationId}/latest")]
        public async Task<IActionResult> GetLatestByStationId(int stationId)
        {
            return Ok(await _stationStatusLogsService.GetLatestByStationIdAsync(stationId));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteLog(int id)
        {
            await _stationStatusLogsService.DeleteAsync(id);
            return Ok($"Log with ID {id} deleted successfully.");
        }
    }
}