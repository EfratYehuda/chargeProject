using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.USERDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // Same idea as DriversController: an owner profile is private to its owner (and admin).
    // Profiles are created through POST api/users/become-owner.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OwnerController : ControllerBase
    {
        private readonly IOwnersService _ownersService;

        public OwnerController(IOwnersService ownersService)
        {
            _ownersService = ownersService;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllOwners()
        {
            return Ok(await _ownersService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOwnerById(int id)
        {
            if (!User.IsAdmin() && User.GetOwnerId() != id)
                throw new NotFoundException("פרופיל בעל העמדות לא נמצא");

            return Ok(await _ownersService.GetByIdAsync(id));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOwnerByUserId(int userId)
        {
            if (!User.IsAdmin() && User.GetUserId() != userId)
                throw new NotFoundException("פרופיל בעל העמדות לא נמצא");

            return Ok(await _ownersService.GetByUserIdAsync(userId));
        }

        // IDOR protection: an owner sees only his own dashboard
        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetDashboard(int id)
        {
            if (!User.IsAdmin() && User.GetOwnerId() != id)
                throw new NotFoundException("פרופיל בעל העמדות לא נמצא");

            return Ok(await _ownersService.GetDashboardAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> CreateOwner([FromBody] CreateOwnerDto dto)
        {
            return Ok(await _ownersService.AddAsync(dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOwner(int id)
        {
            if (!User.IsAdmin() && User.GetOwnerId() != id)
                throw new NotFoundException("פרופיל בעל העמדות לא נמצא");

            await _ownersService.DeleteAsync(id);
            return Ok($"Owner with ID {id} deleted successfully.");
        }
    }
}