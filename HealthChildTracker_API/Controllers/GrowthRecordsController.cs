using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthChildTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrowthRecordsController : ControllerBase
    {
        private readonly IGrowthRecordService _growthRecordService;
        private readonly ILogger<GrowthRecordsController> _logger;

        public GrowthRecordsController(IGrowthRecordService growthRecordService, ILogger<GrowthRecordsController> logger)
        {
            _growthRecordService = growthRecordService ?? throw new ArgumentNullException(nameof(growthRecordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("child/{childId}")]
        public async Task<IActionResult> GetAllGrowthRecordsByChildId(int childId)
        {
            try
            {
                var records = await _growthRecordService.GetAllGrowthRecordsByChildIdAsync(childId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting growth records for child {childId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("Get {recordId}")]
        public async Task<IActionResult> GetGrowthRecordById(int recordId)
        {
            try
            {
                var record = await _growthRecordService.GetGrowthRecordByIdAsync(recordId);
                return Ok(record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting growth record {recordId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("Create new record")]
        public async Task<IActionResult> CreateGrowthRecord([FromBody] CreateGrowthRecordDTO recordDTO)
        {
            try
            {
                var record = await _growthRecordService.CreateGrowthRecordAsync(recordDTO);
                return CreatedAtAction(nameof(GetGrowthRecordById), new { recordId = record.RecordId }, record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating growth record for child {recordDTO.ChildId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{recordId}/Update record")]
        public async Task<IActionResult> UpdateGrowthRecord(int recordId, [FromBody] UpdateGrowthRecordDTO recordDTO)
        {
            try
            {
                var record = await _growthRecordService.UpdateGrowthRecordAsync(recordId, recordDTO);
                return Ok(record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating growth record {recordId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{recordId}/Delete")]
        public async Task<IActionResult> DeleteGrowthRecord(int recordId)
        {
            try
            {
                var result = await _growthRecordService.DeleteGrowthRecordAsync(recordId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting growth record {recordId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
