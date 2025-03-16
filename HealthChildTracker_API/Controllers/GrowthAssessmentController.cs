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
    public class GrowthAssessmentController : ControllerBase
    {
        private readonly IGrowthAssessmentService _assessmentService;
        private readonly IGrowthRecordService _recordService;
        private readonly ILogger<GrowthAssessmentController> _logger;

        public GrowthAssessmentController(
            IGrowthAssessmentService assessmentService,
            IGrowthRecordService recordService,
            ILogger<GrowthAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _recordService = recordService;
            _logger = logger;
        }

        /// <summary>
        /// Đánh giá tăng trưởng dựa trên bản ghi cụ thể
        /// </summary>
        [HttpGet("record/{recordId}")]
        [ProducesResponseType(typeof(GrowthAssessmentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GrowthAssessmentDTO>> AssessGrowthByRecordId(int recordId)
        {
            try
            {
                // Lấy growth record
                var record = await _recordService.GetGrowthRecordByIdAsync(recordId);
                if (record == null)
                {
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng với ID {recordId}");
                }

                // Chuyển đổi từ DTO sang entity để đánh giá
                var recordEntity = new GrowthRecord
                {
                    RecordId = record.RecordId,
                    ChildId = record.ChildId,
                    Height = record.Height,
                    Weight = record.Weight,
                    Bmi = record.Bmi,
                    HeadCircumference = record.HeadCircumference,
                    CreatedAt = record.CreatedAt,
                    UpdatedAt = record.UpdatedAt,
                    Note = record.Note
                };

                // Thực hiện đánh giá
                var assessment = await _assessmentService.AssessGrowthAsync(recordEntity);
                return Ok(assessment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh giá tăng trưởng cho bản ghi {RecordId}", recordId);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }

        /// <summary>
        /// Đánh giá tăng trưởng cho bản ghi mới nhất của trẻ
        /// </summary>
        [HttpGet("child/{childId}/latest")]
        [ProducesResponseType(typeof(GrowthAssessmentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GrowthAssessmentDTO>> AssessLatestGrowthByChildId(int childId)
        {
            try
            {
                // Lấy tất cả bản ghi của trẻ
                var records = await _recordService.GetAllGrowthRecordsByChildIdAsync(childId);
                if (!records.Any())
                {
                    return NotFound($"Không tìm thấy bản ghi tăng trưởng nào cho trẻ với ID {childId}");
                }

                // Lấy bản ghi mới nhất theo CreatedAt
                var latestRecord = records
                    .OrderByDescending(r => r.CreatedAt)
                    .First();

                // Chuyển đổi từ DTO sang entity để đánh giá
                var recordEntity = new GrowthRecord
                {
                    RecordId = latestRecord.RecordId,
                    ChildId = latestRecord.ChildId,
                    Height = latestRecord.Height,
                    Weight = latestRecord.Weight,
                    Bmi = latestRecord.Bmi,
                    HeadCircumference = latestRecord.HeadCircumference,
                    CreatedAt = latestRecord.CreatedAt,
                    UpdatedAt = latestRecord.UpdatedAt,
                    Note = latestRecord.Note
                };

                // Thực hiện đánh giá
                var assessment = await _assessmentService.AssessGrowthAsync(recordEntity);
                return Ok(assessment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh giá tăng trưởng cho trẻ {ChildId}", childId);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }
    }
}
