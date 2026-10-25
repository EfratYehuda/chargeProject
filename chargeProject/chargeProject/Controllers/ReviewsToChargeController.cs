using Microsoft.AspNetCore.Mvc;
using BLL.Repositories.IRepositories;
using DTO.REVIEWDTO;

namespace chargeProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsToChargeController : ControllerBase
    {
        private readonly InterfaceReviewsToCharge _reviewRepository;

        public ReviewsToChargeController(InterfaceReviewsToCharge reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            try { return Ok(await _reviewRepository.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null) return NotFound($"Review with ID {id} not found.");
                return Ok(review);
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            try { return Ok(await _reviewRepository.GetByStationIdAsync(stationId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpGet("station/{stationId}/average-rating")]
        public async Task<IActionResult> GetStationAverageRating(int stationId)
        {
            try { return Ok(await _reviewRepository.GetStationAverageRatingAsync(stationId)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            try { return Ok(await _reviewRepository.AddAsync(dto)); }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var success = await _reviewRepository.DeleteAsync(id);
                if (!success) return NotFound($"Review with ID {id} not found.");
                return Ok($"Review with ID {id} deleted successfully.");
            }
            catch (Exception ex) { return StatusCode(500, $"Error: {ex.Message}"); }
        }
    }
}