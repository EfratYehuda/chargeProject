using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Services;
using DTO.REVIEWDTO;
using System.Security.Claims;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsToChargeController : ControllerBase
    {
        private readonly IReviewsToChargeService _reviewsToChargeService;

        public ReviewsToChargeController(IReviewsToChargeService reviewsToChargeService)
        {
            _reviewsToChargeService = reviewsToChargeService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            try { return Ok(await _reviewsToChargeService.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewsToChargeService.GetByIdAsync(id);
                if (review == null) return NotFound($"Review with ID {id} not found.");
                return Ok(review);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            try { return Ok(await _reviewsToChargeService.GetByStationIdAsync(stationId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [AllowAnonymous]
        [HttpGet("station/{stationId}/average-rating")]
        public async Task<IActionResult> GetStationAverageRating(int stationId)
        {
            try { return Ok(await _reviewsToChargeService.GetStationAverageRatingAsync(stationId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            try { return Ok(await _reviewsToChargeService.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID is missing in token.");
                }

                int? driverId = await _reviewsToChargeService.GetDriverIdByUserIdAsync(userId);
                if (driverId == null) return Unauthorized("User is not a driver.");

                var success = await _reviewsToChargeService.DeleteAsync(id, driverId.Value);
                if (!success) return NotFound($"Review with ID {id} not found or does not belong to you.");

                return Ok($"Review with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}