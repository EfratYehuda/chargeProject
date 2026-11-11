using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.USERDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // A driver profile holds name, email and phone, so it is private:
    // you see your own profile, an admin sees all. Someone else's profile is simply "not found".
    // Profiles are created through POST api/users/become-driver (which also returns the new token),
    // so the old "create a driver for any UserId" endpoint is now admin only.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DriversController : ControllerBase
    {
        private readonly IDriversService _driversService;

        public DriversController(IDriversService driversService)
        {
            _driversService = driversService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllDrivers()
        {
            return Ok(await _driversService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            if (!User.IsAdmin() && User.GetDriverId() != id)
                throw new NotFoundException("פרופיל הנהג לא נמצא");

            return Ok(await _driversService.GetByIdAsync(id));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDriverByUserId(int userId)
        {
            if (!User.IsAdmin() && User.GetUserId() != userId)
                throw new NotFoundException("פרופיל הנהג לא נמצא");

            return Ok(await _driversService.GetByUserIdAsync(userId));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            return Ok(await _driversService.AddAsync(dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            if (!User.IsAdmin() && User.GetDriverId() != id)
                throw new NotFoundException("פרופיל הנהג לא נמצא");

            await _driversService.DeleteAsync(id);
            return Ok($"Driver with ID {id} deleted successfully.");
        }
    }
}