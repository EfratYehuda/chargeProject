using BLL.Exceptions;
using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.USERDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IWebHostEnvironment env,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _env = env;
            _logger = logger;
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

        // POST api/users/forgot-password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var result = await _userService.ForgotPasswordAsync(dto);
            if (!result.SentViaEmail && !string.IsNullOrEmpty(result.DevCode))
            {
                _logger.LogWarning("Password reset code for {Email}: {Code}", dto.Email, result.DevCode);
            }

            return Ok(new
            {
                message = result.SentViaEmail
                    ? "אם כתובת האימייל קיימת במערכת, נשלח אליה קוד איפוס בן 6 ספרות."
                    : "אם כתובת האימייל קיימת במערכת, נוצר קוד איפוס בן 6 ספרות.",
                sentViaEmail = result.SentViaEmail,
                devCode = _env.IsDevelopment() && !result.SentViaEmail ? result.DevCode : null
            });
        }

        // POST api/users/reset-password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var error = await _userService.ResetPasswordAsync(dto);
            if (error != null)
                return BadRequest(new { detail = error });

            return Ok(new { message = "הסיסמה אופסה בהצלחה. כעת ניתן להתחבר עם הסיסמה החדשה." });
        }

        // ---------- Logged-in user ----------

        // GET api/users/me - shows the claims inside my own token (handy for debugging)
        [HttpGet("me")]
        public IActionResult Me() => Ok(User.Claims.Select(c => new { c.Type, c.Value }));

        // POST api/users/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await _userService.ChangePasswordAsync(User.GetUserId(), dto);
            return Ok(new { message = "הסיסמה עודכנה בהצלחה." });
        }

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