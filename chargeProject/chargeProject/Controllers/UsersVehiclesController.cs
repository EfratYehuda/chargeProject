using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.IRepositories;
using DTO.VEHICLESDTO;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersVehiclesController : ControllerBase
    {
        private readonly InterfaceUsersVehicles _vehicleRepository;

        public UsersVehiclesController(InterfaceUsersVehicles vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            try { return Ok(await _vehicleRepository.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(id);
                if (vehicle == null) return NotFound($"Vehicle with ID {id} not found.");
                return Ok(vehicle);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("driver/{driverId}")]
        public async Task<IActionResult> GetByDriverId(int driverId)
        {
            try { return Ok(await _vehicleRepository.GetByDriverIdAsync(driverId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateUpdateVehicleDto dto)
        {
            try { return Ok(await _vehicleRepository.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateUpdateVehicleDto dto)
        {
            try
            {
                var updated = await _vehicleRepository.UpdateAsync(id, dto);
                if (updated == null) return NotFound($"Vehicle with ID {id} not found.");
                return Ok(updated);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var success = await _vehicleRepository.DeleteAsync(id);
                if (!success) return NotFound($"Vehicle with ID {id} not found.");
                return Ok($"Vehicle with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}