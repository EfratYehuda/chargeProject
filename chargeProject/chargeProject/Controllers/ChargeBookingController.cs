using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.IRepositories;
using DTO.VEHICLESDTO;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargeBookingController : ControllerBase
    {
        private readonly InterfaceChargeBooking _bookingRepository;

        public ChargeBookingController(InterfaceChargeBooking bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        // 1. GET: api/ChargeBooking - שליפת כל ההזמנות
        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingRepository.GetAllAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 2. GET: api/ChargeBooking/5 - שליפת הזמנה לפי מזהה
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
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
                var createdBooking = await _bookingRepository.AddAsync(newBookingDto);
                return Ok(createdBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 4. PUT: api/ChargeBooking/5 - עדכון הזמנה קיימת
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] CreateChargeBookingDto updatedBookingDto)
        {
            if (updatedBookingDto == null)
            {
                return BadRequest("Booking data is invalid.");
            }

            try
            {
                var updatedBooking = await _bookingRepository.UpdateAsync(id, updatedBookingDto);
                if (updatedBooking == null)
                {
                    return NotFound($"Booking with ID {id} was not found for update.");
                }
                return Ok(updatedBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 5. PATCH: api/ChargeBooking/5/status - עדכון סטטוס בלבד
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromQuery] string status)
        {
            try
            {
                var updatedBooking = await _bookingRepository.UpdateStatusAsync(id, status);
                if (updatedBooking == null)
                {
                    return NotFound($"Booking with ID {id} was not found.");
                }
                return Ok(updatedBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        // 6. DELETE: api/ChargeBooking/5 - מחיקת הזמנה
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var success = await _bookingRepository.DeleteAsync(id);
                if (!success)
                {
                    return NotFound($"Booking with ID {id} was not found.");
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