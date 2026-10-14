using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.IRepositories;
using DTO.USERDTO;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly InterfaceDriver _driverRepository;

        public DriversController(InterfaceDriver driverRepository)
        {
            _driverRepository = driverRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            try { return Ok(await _driverRepository.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            try
            {
                var driver = await _driverRepository.GetByIdAsync(id);
                if (driver == null) return NotFound($"Driver with ID {id} not found.");
                return Ok(driver);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetDriverByUserId(int userId)
        {
            try
            {
                var driver = await _driverRepository.GetByUserIdAsync(userId);
                if (driver == null) return NotFound($"Driver with User ID {userId} not found.");
                return Ok(driver);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try { return Ok(await _driverRepository.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var success = await _driverRepository.DeleteAsync(id);
                if (!success) return NotFound($"Driver with ID {id} not found.");
                return Ok($"Driver with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}