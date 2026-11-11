using BLL.Services;
using chargeProject.Extensions;
using DTO.STATIONDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // Reading stations is allowed for any logged-in user (drivers search for them).
    // Changing a station requires the "Owner" policy, and the ownerId comes from the token,
    // so an owner can only ever touch his own stations.
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

        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            return Ok(await _chargingStationsService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(int id)
        {
            return Ok(await _chargingStationsService.GetByIdAsync(id));
        }

        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetStationsByOwner(int ownerId)
        {
            return Ok(await _chargingStationsService.GetByOwnerIdAsync(ownerId));
        }

        [HttpGet("radius")]
        public async Task<IActionResult> GetActiveInRadius([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius, [FromQuery] string? chargerType)
        {
            return Ok(await _chargingStationsService.GetActiveInRadiusAsync(lat, lng, radius, chargerType));
        }

        [HttpPost]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> CreateStation([FromBody] CreateUpdateStationDto dto)
        {
            return Ok(await _chargingStationsService.AddAsync(User.GetRequiredOwnerId(), dto));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> UpdateStation(int id, [FromBody] CreateUpdateStationDto dto)
        {
            return Ok(await _chargingStationsService.UpdateAsync(id, User.GetRequiredOwnerId(), dto));
        }

        [HttpPatch("{id}/status")]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive)
        {
            return Ok(await _chargingStationsService.SetActiveAsync(id, User.GetRequiredOwnerId(), isActive));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            await _chargingStationsService.DeleteAsync(id, User.GetRequiredOwnerId());
            return Ok($"Station with ID {id} deleted successfully.");
        }
    }
}