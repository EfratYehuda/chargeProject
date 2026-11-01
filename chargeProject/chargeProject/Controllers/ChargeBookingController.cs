using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTO.VEHICLESDTO;
using BLL.Services;
using System.Security.Claims;

namespace chargeProject.Controllers
{
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

        // 1. GET: api/ChargeBooking - שליפת כל ההזמנות
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                var bookings = await _chargeBookingService.GetAllAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 2. GET: api/ChargeBooking/5 - שליפת הזמנה לפי מזהה
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                var booking = await _chargeBookingService.GetByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} was not found.");
                }
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 3. POST: api/ChargeBooking - יצירת שריון/הזמנה חדשה
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateChargeBookingDto newBookingDto)
        {
            if (newBookingDto == null)
            {
                return BadRequest("Booking data is empty.");
            }

            try
            {
                var createdBooking = await _chargeBookingService.AddAsync(newBookingDto);
                return Ok(createdBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 4. PUT: api/ChargeBooking/5 - עדכון הזמנה קיימת (מאובטח IDOR)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] CreateChargeBookingDto updatedBookingDto)
        {
            if (updatedBookingDto == null)
            {
                return BadRequest("Booking data is invalid.");
            }

            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? driverId = await _chargeBookingService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var updatedBooking = await _chargeBookingService.UpdateAsync(id, driverId.Value, updatedBookingDto);
                if (updatedBooking == null)
                {
                    return NotFound($"Booking with ID {id} was not found or does not belong to you.");
                }
                return Ok(updatedBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 5. PATCH: api/ChargeBooking/5/status - עדכון סטטוס בלבד (מאובטח IDOR)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromQuery] string status)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? driverId = await _chargeBookingService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var updatedBooking = await _chargeBookingService.UpdateStatusAsync(id, driverId.Value, status);
                if (updatedBooking == null)
                {
                    return NotFound($"Booking with ID {id} was not found or does not belong to you.");
                }
                return Ok(updatedBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 6. DELETE: api/ChargeBooking/5 - מחיקת הזמנה (מאובטח IDOR)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? driverId = await _chargeBookingService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var success = await _chargeBookingService.DeleteAsync(id, driverId.Value);
                if (!success)
                {
                    return NotFound($"Booking with ID {id} was not found or does not belong to you.");
                }

                return Ok($"Booking with ID {id} was deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}