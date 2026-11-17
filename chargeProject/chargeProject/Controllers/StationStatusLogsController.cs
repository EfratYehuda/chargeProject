using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.STATIONDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StationStatusLogsController : ControllerBase
    {
        private readonly IStationStatusLogsService _stationStatusLogsService;
        private readonly IChargingStationsService _chargingStationsService;

        public StationStatusLogsController(
            IStationStatusLogsService stationStatusLogsService,
            IChargingStationsService chargingStationsService)
        {
            _stationStatusLogsService = stationStatusLogsService;
            _chargingStationsService = chargingStationsService;
        }

        public class ReportFaultRequest
        {
            [Required]
            public int StationId { get; set; }
        }

        // נהג מדווח על תקלה בעמדה -> מסמן את העמדה כ-Faulty (חסימת דיווח עצמי)
        [HttpPost("report-fault")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> ReportFault([FromBody] ReportFaultRequest request)
        {
            var station = await _chargingStationsService.GetByIdAsync(request.StationId);
            if (User.GetOwnerId() == station.OwnerId)
                throw new BusinessRuleException("לא ניתן לדווח תקלה על עמדה שבבעלותך");

            if (station.ApprovalStatus != StationApprovalStatuses.Approved)
                throw new BusinessRuleException("העמדה טרם אושרה במערכת");

            var dto = new CreateStationStatusLogDto
            {
                StationId = request.StationId,
                Status = StationStatuses.Faulty,
                CurrentDriverId = User.GetRequiredDriverId()
            };
            return Ok(await _stationStatusLogsService.AddAsync(dto));
        }

        // בעל התחנה מסמן שהתקלה תוקנה -> מחזיר את העמדה לסטטוס Available
        [HttpPost("station/{stationId}/resolve")]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> ResolveFault(int stationId)
        {
            var station = await _chargingStationsService.GetByIdAsync(stationId);
            if (station.OwnerId != User.GetRequiredOwnerId())
                throw new NotFoundException("העמדה לא נמצאה");

            var dto = new CreateStationStatusLogDto
            {
                StationId = stationId,
                Status = StationStatuses.Available,
                CurrentDriverId = null
            };
            return Ok(await _stationStatusLogsService.AddAsync(dto));
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