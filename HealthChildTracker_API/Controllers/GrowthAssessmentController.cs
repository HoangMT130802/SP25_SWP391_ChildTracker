using AutoMapper;
using BusinessLogic.DTOs.GrowthAssessment;
using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DataAccess.UnitOfWork;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GrowthAssessmentController : ControllerBase
    {
        private readonly IGrowthRecordService _growthRecordService;
        private readonly IGrowthAssessmentService _growthAssessmentService;
        private readonly IChildService _childService;
        private readonly IMapper _mapper;
        private readonly ILogger<GrowthAssessmentController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GrowthAssessmentController(
            IGrowthRecordService growthRecordService,
            IGrowthAssessmentService growthAssessmentService,
            IChildService childService,
            IMapper mapper,
            ILogger<GrowthAssessmentController> logger,
            IUnitOfWork unitOfWork)
        {
            _growthRecordService = growthRecordService;
            _growthAssessmentService = growthAssessmentService;
            _childService = childService;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        private async Task<bool> ValidateChildAccess(int childId)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    return false;
                }

                if (User.IsInRole("Admin") || User.IsInRole("Doctor"))
                {
                    return true;
                }

                var child = await _childService.GetChildByIdAsync(childId, currentUserId);
                return child != null;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("assess/{childId}")]
        public async Task<IActionResult> CreateAssessment(int childId)
        {
            try
            {
                if (!await ValidateChildAccess(childId))
                {
                    return Forbid("Bạn không có quyền truy cập thông tin của trẻ này");
                }

                var records = await _growthRecordService.GetAllGrowthRecordsByChildIdAsync(childId);
                var latestRecordDTO = records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

                if (latestRecordDTO == null)
                {
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng nào cho trẻ {childId}");
                }

                var recordRepository = _unitOfWork.GetRepository<GrowthRecord>();
                var recordEntity = await recordRepository.GetAsync(r => r.RecordId == latestRecordDTO.RecordId);

                if (recordEntity == null)
                {
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng trong database");
                }

                var assessment = await _growthAssessmentService.AssessGrowthAsync(recordEntity);

                return Ok(new
                {
                    LatestMeasurement = latestRecordDTO,
                    Assessment = assessment,
                    Recommendations = assessment.Recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tạo đánh giá cho trẻ {childId}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("latest/{childId}")]
        public async Task<IActionResult> GetLatestAssessment(int childId)
        {
            try
            {
                if (!await ValidateChildAccess(childId))
                {
                    return Forbid("Bạn không có quyền truy cập thông tin của trẻ này");
                }

                var records = await _growthRecordService.GetAllGrowthRecordsByChildIdAsync(childId);
                var latestRecordDTO = records.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

                if (latestRecordDTO == null)
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng cho trẻ {childId}");

                var recordRepository = _unitOfWork.GetRepository<GrowthRecord>();
                var recordEntity = await recordRepository.GetAsync(r => r.RecordId == latestRecordDTO.RecordId);

                if (recordEntity == null)
                {
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng trong database");
                }

                var assessment = await _growthAssessmentService.AssessGrowthAsync(recordEntity);

                return Ok(new
                {
                    LatestMeasurement = latestRecordDTO,
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
