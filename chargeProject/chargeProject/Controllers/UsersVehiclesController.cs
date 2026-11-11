using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.VEHICLESDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // A vehicle belongs to one driver. Only that driver (or an admin) can read it,
    // and only that driver can change it. The driverId always comes from the token.
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

        // Every vehicle of every driver: admin only
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllVehicles()
        {
            return Ok(await _usersVehiclesService.GetAllAsync());
        }

        // The logged-in driver's vehicles
        [HttpGet("my")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> GetMyVehicles()
        {
            return Ok(await _usersVehiclesService.GetByDriverIdAsync(User.GetRequiredDriverId()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            if (User.IsAdmin())
                return Ok(await _usersVehiclesService.GetByIdAsync(id));

            return Ok(await _usersVehiclesService.GetOwnedAsync(id, User.GetRequiredDriverId()));
        }

        // Kept for compatibility with the old route, but a driver may ask only for himself
        [HttpGet("driver/{driverId}")]
        public async Task<IActionResult> GetByDriverId(int driverId)
        {
            if (!User.IsAdmin() && User.GetDriverId() != driverId)
                throw new NotFoundException("הנהג לא נמצא");

            return Ok(await _usersVehiclesService.GetByDriverIdAsync(driverId));
        }

        [HttpPost]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateUpdateVehicleDto dto)
        {
            return Ok(await _usersVehiclesService.AddAsync(User.GetRequiredDriverId(), dto));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateUpdateVehicleDto dto)
        {
            return Ok(await _usersVehiclesService.UpdateAsync(id, User.GetRequiredDriverId(), dto));
        }

        [HttpPatch("{id}/battery")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> UpdateBattery(int id, [FromQuery] double batteryPercent)
        {
            return Ok(await _usersVehiclesService.UpdateCurrentBatteryAsync(id, User.GetRequiredDriverId(), batteryPercent));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            await _usersVehiclesService.DeleteAsync(id, User.GetRequiredDriverId());
            return Ok($"Vehicle with ID {id} deleted successfully.");
        }
    }
}