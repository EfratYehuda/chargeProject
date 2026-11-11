using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.USERDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ---------- Public endpoints (no token needed) ----------

        // POST api/users/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            return Ok(await _userService.AddAsync(dto));
        }

        // POST api/users/login
        // Wrong email/password is an expected result, so it is a plain 401 and not an exception.
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            if (result is null)
                return Unauthorized("Wrong email or password.");

            return Ok(result);
        }

        // ---------- Logged-in user ----------

        // GET api/users/me - shows the claims inside my own token (handy for debugging)
        [HttpGet("me")]
        public IActionResult Me() => Ok(User.Claims.Select(c => new { c.Type, c.Value }));

        // POST api/users/become-owner - creates the profile and returns a NEW token that contains ownerId
        [HttpPost("become-owner")]
        public async Task<IActionResult> BecomeOwner()
        {
            return Ok(await _userService.BecomeOwnerAsync(User.GetUserId()));
        }

        // POST api/users/become-driver - creates the profile and returns a NEW token that contains driverId
        [HttpPost("become-driver")]
        public async Task<IActionResult> BecomeDriver()
        {
            return Ok(await _userService.BecomeDriverAsync(User.GetUserId()));
        }

        // ---------- Admin only ----------

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _userService.GetAllAsync());
        }

        // ---------- Me or admin ----------

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            EnsureSelfOrAdmin(id);
            return Ok(await _userService.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserProfileDto dto)
        {
            EnsureSelfOrAdmin(id);
            return Ok(await _userService.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            EnsureSelfOrAdmin(id);
            await _userService.DeleteAsync(id);
            return Ok($"User with ID {id} deleted successfully.");
        }

        // Someone else's user id is reported as "not found", so IDs cannot be probed.
        private void EnsureSelfOrAdmin(int id)
        {
            if (!User.IsAdmin() && User.GetUserId() != id)
                throw new NotFoundException("המשתמש לא נמצא");
        }
    }
}