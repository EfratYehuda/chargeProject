using BLL.Services;
using DAL.Repositories.IRepositories;
using DTO.USERDTO;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IDriversService _driversService;

        public DriversController(IDriversService driversService)
        {
           _driversService = driversService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            try { return Ok(await _driversService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            try
            {
                var driver = await _driversService.GetByIdAsync(id);
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
                var driver = await _driversService.GetByUserIdAsync(userId);
                if (driver == null) return NotFound($"Driver with User ID {userId} not found.");
                return Ok(driver);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try { return Ok(await _driversService.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var success = await _driversService.DeleteAsync(id);
                if (!success) return NotFound($"Driver with ID {id} not found.");
                return Ok($"Driver with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}