using BLL.Services;
using chargeProject.Extensions;
using DTO;
using DTO.REVIEWDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chargeProject.Controllers
{
    // Reviews of a station are public to any logged-in user. Writing is for drivers only,
    // and deleting is allowed for the review's author or an Admin.
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

        // Every review in the system: admin only
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllReviews()
        {
            return Ok(await _reviewsToChargeService.GetAllAsync());
        }

        // The reviews written by the logged-in driver
        [HttpGet("my")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> GetMyReviews()
        {
            return Ok(await _reviewsToChargeService.GetByDriverIdAsync(User.GetRequiredDriverId()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            return Ok(await _reviewsToChargeService.GetByIdAsync(id));
        }

        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            return Ok(await _reviewsToChargeService.GetByStationIdAsync(stationId));
        }

        [HttpGet("station/{stationId}/average-rating")]
        public async Task<IActionResult> GetStationAverageRating(int stationId)
        {
            return Ok(await _reviewsToChargeService.GetStationAverageRatingAsync(stationId));
        }

        [HttpPost]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            return Ok(await _reviewsToChargeService.AddAsync(User.GetRequiredDriverId(), dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (User.IsAdmin())
            {
                await _reviewsToChargeService.DeleteAsAdminAsync(id);
            }
            else
            {
                await _reviewsToChargeService.DeleteAsync(id, User.GetRequiredDriverId());
            }

            return Ok($"Review with ID {id} deleted successfully.");
        }
    }
}