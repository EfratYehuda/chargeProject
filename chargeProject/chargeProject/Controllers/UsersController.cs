using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.IRepositories;
using DTO.USERDTO;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly InterfaceUser _userRepository;

        public UsersController(InterfaceUser userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try { return Ok(await _userRepository.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null) return NotFound($"User with ID {id} not found.");
                return Ok(user);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto, [FromQuery] string passwordHash)
        {
            try { return Ok(await _userRepository.AddAsync(dto, passwordHash)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                var updated = await _userRepository.UpdateAsync(id, dto);
                if (updated == null) return NotFound($"User with ID {id} not found.");
                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var success = await _userRepository.DeleteAsync(id);
                if (!success) return NotFound($"User with ID {id} not found.");
                return Ok($"User with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}