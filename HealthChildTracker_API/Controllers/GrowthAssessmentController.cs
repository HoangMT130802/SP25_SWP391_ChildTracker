using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrowthAssessmentController : ControllerBase
    {
        private readonly IGrowthRecordService _growthRecordService;
        private readonly IGrowthAssessmentService _growthAssessmentService;
        private readonly ILogger<GrowthAssessmentController> _logger;

        public GrowthAssessmentController(
            IGrowthRecordService growthRecordService,
            IGrowthAssessmentService growthAssessmentService,
            ILogger<GrowthAssessmentController> logger)
        {
            _growthRecordService = growthRecordService;
            _growthAssessmentService = growthAssessmentService;
            _logger = logger;
        }

        [HttpPost("assess/{childId}")]
        public async Task<ActionResult<GrowthAssessmentDTO>> AssessGrowth(
            int childId,
            [FromBody] CreateGrowthRecordDTO recordDTO)
        {
            try
            {
                // Lưu bản ghi tăng trưởng
                var savedRecord = await _growthRecordService.CreateGrowthRecordAsync(recordDTO);

                // Đánh giá tăng trưởng
                var assessment = await _growthAssessmentService.AssessGrowthAsync(childId, savedRecord);

                return Ok(assessment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh giá tăng trưởng");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }
    }
}
