using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.VEHICLESDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // Security model for this controller:
    //  1. [Authorize] on the class        -> you must be logged in (401 otherwise)
    //  2. Policy "Driver"                  -> your token must contain a driverId claim (403 otherwise)
    //  3. The service gets driverId FROM THE TOKEN and looks the booking up with it,
    //     so a booking that is not yours is "not found" (404). The client never says who it is.
    // Errors: no try-catch. Services throw NotFoundException / ConflictException / BusinessRuleException
    // and GlobalExceptionHandler turns them into 404 / 409 / 400.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChargeBookingController : ControllerBase
    {
        private readonly IChargeBookingService _chargeBookingService;

        public ChargeBookingController(IChargeBookingService chargeBookingService)
        {
            _chargeBookingService = chargeBookingService;
        }

        // GET api/ChargeBooking - every booking of every driver: admin only
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllBookings()
        {
            return Ok(await _chargeBookingService.GetAllAsync());
        }

        // GET api/ChargeBooking/my - the logged-in driver's bookings
        [HttpGet("my")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> GetMyBookings()
        {
            return Ok(await _chargeBookingService.GetByDriverIdAsync(User.GetRequiredDriverId()));
        }

        // GET api/ChargeBooking/my-stations - bookings made at the logged-in owner's stations
        [HttpGet("my-stations")]
        [Authorize(Policy = "Owner")]
        public async Task<IActionResult> GetBookingsAtMyStations()
        {
            return Ok(await _chargeBookingService.GetByOwnerIdAsync(User.GetRequiredOwnerId()));
        }

        // GET api/ChargeBooking/5 - a driver sees only his own booking; admin sees any
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            if (User.IsAdmin())
                return Ok(await _chargeBookingService.GetByIdAsync(id));

            return Ok(await _chargeBookingService.GetOwnedAsync(id, User.GetRequiredDriverId()));
        }

        // POST api/ChargeBooking - the driver comes from the token
        [HttpPost]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateChargeBookingDto newBookingDto)
        {
            return Ok(await _chargeBookingService.AddAsync(User.GetRequiredDriverId(), newBookingDto));
        }

        // PUT api/ChargeBooking/5
        [HttpPut("{id}")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] CreateChargeBookingDto updatedBookingDto)
        {
            return Ok(await _chargeBookingService.UpdateAsync(id, User.GetRequiredDriverId(), updatedBookingDto));
        }

        // PATCH api/ChargeBooking/5/status?status=Confirmed
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromQuery] string status)
        {
            return Ok(await _chargeBookingService.UpdateStatusAsync(id, User.GetRequiredDriverId(), status));
        }

        // DELETE api/ChargeBooking/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            await _chargeBookingService.DeleteAsync(id, User.GetRequiredDriverId());
            return Ok($"Booking with ID {id} was deleted successfully.");
        }
    }
}