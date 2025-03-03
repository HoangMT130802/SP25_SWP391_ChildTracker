using AutoMapper;
using BusinessLogic.DTOs.GrowthAssessment;
using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
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
        private readonly IMapper _mapper;
        private readonly ILogger<GrowthAssessmentController> _logger;

        public GrowthAssessmentController(
            IGrowthRecordService growthRecordService,
            IGrowthAssessmentService growthAssessmentService,
            IMapper mapper,
            ILogger<GrowthAssessmentController> logger)
        {
            _growthRecordService = growthRecordService;
            _growthAssessmentService = growthAssessmentService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("record-and-assess")]
        public async Task<IActionResult> CreateAndAssessGrowthRecord([FromBody] CreateGrowthRecordDTO recordDTO)
        {
            try
            {
                // 1. Tạo bản ghi mới
                var recordResult = await _growthRecordService.CreateGrowthRecordAsync(recordDTO);

                // 2. Chuyển đổi DTO sang Entity để đánh giá
                var recordEntity = _mapper.Map<GrowthRecord>(recordResult);

                // 3. Đánh giá
                var assessment = await _growthAssessmentService.AssessGrowthAsync(recordEntity);

                return Ok(new
                {
                    Record = recordResult,
                    Assessment = assessment,
                    Recommendations = assessment.Recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo và đánh giá bản ghi tăng trưởng");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("latest/{childId}")]
        public async Task<IActionResult> GetLatestAssessment(int childId)
        {
            try
            {
                var records = await _growthRecordService.GetAllGrowthRecordsByChildIdAsync(childId);
                var latestRecord = records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

                if (latestRecord == null)
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng cho trẻ {childId}");

                // Chuyển đổi DTO sang Entity
                var recordEntity = _mapper.Map<GrowthRecord>(latestRecord);
                var assessment = await _growthAssessmentService.AssessGrowthAsync(recordEntity);

                return Ok(new
                {
                    LatestMeasurement = latestRecord,
                    Assessment = assessment,
                    Recommendations = assessment.Recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy đánh giá mới nhất cho trẻ {childId}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("history/{childId}")]
        public async Task<IActionResult> GetAssessmentHistory(
            int childId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var records = await _growthRecordService.GetAllGrowthRecordsByChildIdAsync(childId);

                var filteredRecords = records
                    .Where(r => (!startDate.HasValue || r.CreatedAt >= startDate) &&
                               (!endDate.HasValue || r.CreatedAt <= endDate))
                    .OrderBy(r => r.CreatedAt)
                    .ToList();

                if (!filteredRecords.Any())
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng cho trẻ {childId} trong khoảng thời gian yêu cầu");

                var assessments = new List<object>();
                foreach (var record in filteredRecords)
                {
                    var recordEntity = _mapper.Map<GrowthRecord>(record);
                    var assessment = await _growthAssessmentService.AssessGrowthAsync(recordEntity);
                    assessments.Add(new
                    {
                        Date = record.CreatedAt,
                        Measurement = record,
                        Assessment = assessment,
                        Recommendations = assessment.Recommendations
                    });
                }

                var trend = new
                {
                    HeightChange = filteredRecords.Last().Height - filteredRecords.First().Height,
                    WeightChange = filteredRecords.Last().Weight - filteredRecords.First().Weight,
                    BMIChange = filteredRecords.Last().Bmi - filteredRecords.First().Bmi,
                    TimePeriod = (filteredRecords.Last().CreatedAt - filteredRecords.First().CreatedAt).Days / 30.0,
                    NumberOfRecords = filteredRecords.Count
                };

                return Ok(new
                {
                    ChildId = childId,
                    Assessments = assessments,
                    Trend = trend,
                    StartDate = filteredRecords.First().CreatedAt,
                    EndDate = filteredRecords.Last().CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch sử đánh giá cho trẻ {childId}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("assess/{recordId}")]
        public async Task<IActionResult> AssessSpecificRecord(int recordId)
        {
            try
            {
                var recordDTO = await _growthRecordService.GetGrowthRecordByIdAsync(recordId);
                if (recordDTO == null)
                    return NotFound($"Không tìm thấy bản ghi {recordId}");

                var recordEntity = _mapper.Map<GrowthRecord>(recordDTO);
                var assessment = await _growthAssessmentService.AssessGrowthAsync(recordEntity);

                return Ok(new
                {
                    Record = recordDTO,
                    Assessment = assessment,
                    Recommendations = assessment.Recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi đánh giá bản ghi {recordId}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}
