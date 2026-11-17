using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.STATIONDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // --- המלצה – מצב רדיוס: עמדות סביב הנהג, ממוינות לפי אחוז התאמה ---
        [AllowAnonymous]
        [HttpPost("recommend")]
        public async Task<ActionResult<List<RankedStationResultDto>>> RecommendStations(
            [FromBody] DriverPreferenceRequestDto request)
        {
            return Ok(await _chargingStationsService.RecommendAsync(request));
        }

        // --- המלצה – מצב מסלול: איפה הכי משתלם לעצור בדרך מהמוצא ליעד ---
        [AllowAnonymous]
        [HttpPost("recommend-route")]
        public async Task<ActionResult<RouteRecommendationResponseDto>> RecommendRoute(
            [FromBody] DriverPreferenceRequestDto request)
        {
            return Ok(await _chargingStationsService.RecommendRouteAsync(request));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            return Ok(await _chargingStationsService.GetAllAsync());
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllStationsForAdmin()
        {
            return Ok(await _chargingStationsService.GetAllForAdminAsync());
        }

        [HttpGet("admin/pending")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetPendingStationsForAdmin()
        {
            return Ok(await _chargingStationsService.GetPendingForAdminAsync());
        }

        [HttpPatch("{id}/approval")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ReviewStationApproval(int id, [FromBody] ReviewStationApprovalDto dto)
        {
            return Ok(await _chargingStationsService.ReviewApprovalAsync(id, dto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(int id)
        {
            return Ok(await _chargingStationsService.GetByIdAsync(id));
        }

        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetStationsByOwner(int ownerId)
        {
            if (!User.IsAdmin() && User.GetOwnerId() != ownerId)
                throw new NotFoundException("פרופיל בעל העמדות לא נמצא");

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

        [HttpPatch("{id}/blocked-hours")]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> UpdateBlockedHours(int id, [FromBody] UpdateBlockedHoursDto dto)
        {
            return Ok(await _chargingStationsService.UpdateBlockedHoursAsync(id, User.GetRequiredOwnerId(), dto.BlockedHours));
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