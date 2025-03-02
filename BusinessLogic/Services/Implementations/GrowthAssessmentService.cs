using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class GrowthAssessmentService : IGrowthAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GrowthAssessmentService> _logger;

        public GrowthAssessmentService(IUnitOfWork unitOfWork, ILogger<GrowthAssessmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GrowthAssessmentDTO> AssessGrowthAsync(int childId, GrowthRecordDTO record)
        {
            try
            {
                var childRepo = _unitOfWork.GetRepository<Child>();
                var child = await childRepo.GetAsync(c => c.ChildId == childId);

                if (child == null)
                    throw new KeyNotFoundException($"Không tìm thấy trẻ với ID {childId}");

                // Tính tuổi chính xác tại thời điểm đo
                decimal exactAgeInMonths = CalculateExactAgeInMonths(child.DateOfBirth, record.CreatedAt);

                // Lấy các tiêu chuẩn và nội suy
                var standards = await GetInterpolatedStandards(
                    child.Gender,
                    exactAgeInMonths,
                    new[] { "Height", "Weight", "BMI", "HeadCircumference" }
                );

                var assessment = new GrowthAssessmentDTO
                {
                    ExactAgeInMonths = exactAgeInMonths,
                    MeasurementDate = record.CreatedAt,

                    // Tính toán Z-score cho từng chỉ số
                    ZScoreHeight = CalculateZScore(
                        record.Height,
                        standards["Height"].Median,
                        (standards["Height"].Sd1pos - standards["Height"].Median)
                    ),

                    ZScoreWeight = CalculateZScore(
                        record.Weight,
                        standards["Weight"].Median,
                        (standards["Weight"].Sd1pos - standards["Weight"].Median)
                    ),

                    ZScoreBMI = CalculateZScore(
                        record.Bmi,
                        standards["BMI"].Median,
                        (standards["BMI"].Sd1pos - standards["BMI"].Median)
                    ),

                    ZScoreHeadCircumference = CalculateZScore(
                        record.HeadCircumference,
                        standards["HeadCircumference"].Median,
                        (standards["HeadCircumference"].Sd1pos - standards["HeadCircumference"].Median)
                    )
                };

                // Thêm lịch sử tăng trưởng để theo dõi xu hướng
                var growthHistory = await GetGrowthHistory(childId, record.CreatedAt);
                assessment.GrowthTrend = AnalyzeGrowthTrend(growthHistory);

                // Đánh giá tình trạng
                assessment.HeightStatus = GetNutritionalStatus(assessment.ZScoreHeight, "Height");
                assessment.WeightStatus = GetNutritionalStatus(assessment.ZScoreWeight, "Weight");
                assessment.BMIStatus = GetNutritionalStatus(assessment.ZScoreBMI, "BMI");
                assessment.HeadCircumferenceStatus = GetNutritionalStatus(
                    assessment.ZScoreHeadCircumference,
                    "HeadCircumference"
                );

                // Đưa ra lời khuyên dựa trên cả tình trạng hiện tại và xu hướng
                assessment.Recommendations = GetRecommendations(assessment);

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh giá tăng trưởng");
                throw;
            }
        }

        private decimal CalculateExactAgeInMonths(DateTime dateOfBirth, DateTime measurementDate)
        {
            var timeSpan = measurementDate - dateOfBirth;
            return (decimal)timeSpan.TotalDays / 30.44M; // Số ngày trung bình trong một tháng
        }

        private async Task<Dictionary<string, GrowthStandard>> GetInterpolatedStandards(
            string gender,
            decimal exactAgeInMonths,
            string[] measurements)
        {
            var result = new Dictionary<string, GrowthStandard>();
            var standardRepo = _unitOfWork.GetRepository<GrowthStandard>();

            foreach (var measurement in measurements)
            {
                // Lấy các tiêu chuẩn gần nhất
                var lowerAge = (int)Math.Floor(exactAgeInMonths);
                var upperAge = (int)Math.Ceiling(exactAgeInMonths);

                var lowerStandard = await standardRepo.GetAsync(s =>
                    s.Gender == gender &&
                    s.AgeInMonths == lowerAge &&
                    s.Measurement == measurement);

                var upperStandard = await standardRepo.GetAsync(s =>
                    s.Gender == gender &&
                    s.AgeInMonths == upperAge &&
                    s.Measurement == measurement);

                if (lowerStandard == null || upperStandard == null)
                {
                    throw new InvalidOperationException(
                        $"Không tìm thấy dữ liệu chuẩn cho {measurement} ở độ tuổi {exactAgeInMonths} tháng"
                    );
                }

                // Nội suy tuyến tính
                var fraction = exactAgeInMonths - lowerAge;
                result[measurement] = new GrowthStandard
                {
                    Gender = gender,
                    AgeInMonths = (int)exactAgeInMonths,
                    Measurement = measurement,
                    Sd3neg = InterpolateValue(lowerStandard.Sd3neg, upperStandard.Sd3neg, fraction),
                    Sd2neg = InterpolateValue(lowerStandard.Sd2neg, upperStandard.Sd2neg, fraction),
                    Sd1neg = InterpolateValue(lowerStandard.Sd1neg, upperStandard.Sd1neg, fraction),
                    Median = InterpolateValue(lowerStandard.Median, upperStandard.Median, fraction),
                    Sd1pos = InterpolateValue(lowerStandard.Sd1pos, upperStandard.Sd1pos, fraction),
                    Sd2pos = InterpolateValue(lowerStandard.Sd2pos, upperStandard.Sd2pos, fraction),
                    Sd3pos = InterpolateValue(lowerStandard.Sd3pos, upperStandard.Sd3pos, fraction)
                };
            }

            return result;
        }

        private decimal InterpolateValue(decimal start, decimal end, decimal fraction)
        {
            return start + (end - start) * fraction;
        }

        private async Task<List<GrowthRecordDTO>> GetGrowthHistory(int childId, DateTime currentDate)
        {
            // Lấy lịch sử 6 tháng gần nhất
            var recordRepo = _unitOfWork.GetRepository<GrowthRecord>();
            var records = await recordRepo.FindAsync(
                r => r.ChildId == childId &&
                     r.CreatedAt < currentDate &&
                     r.CreatedAt >= currentDate.AddMonths(-6)
            );

            return _mapper.Map<List<GrowthRecordDTO>>(records);
        }

        private GrowthTrend AnalyzeGrowthTrend(List<GrowthRecordDTO> history)
        {
            if (!history.Any()) return new GrowthTrend();

            // Phân tích xu hướng tăng trưởng
            return new GrowthTrend
            {
                HeightVelocity = CalculateGrowthVelocity(history.Select(h => h.Height).ToList()),
                WeightVelocity = CalculateGrowthVelocity(history.Select(h => h.Weight).ToList()),
                IsAccelerating = CheckAcceleration(history),
                ConsistentGrowth = CheckConsistency(history)
            };
        }

        private decimal CalculateGrowthVelocity(List<decimal> measurements)
        {
            if (measurements.Count < 2) return 0;

            // Tính tốc độ tăng trưởng trung bình
            var changes = new List<decimal>();
            for (int i = 1; i < measurements.Count; i++)
            {
                changes.Add(measurements[i] - measurements[i - 1]);
            }
            return changes.Average();
        }

        private string GetDetailedRecommendations(GrowthAssessmentDTO assessment)
        {
            var recommendations = new List<string>();

            // Đánh giá dựa trên xu hướng
            if (assessment.GrowthTrend.HeightVelocity < 0.5M)
            {
                recommendations.Add("- Tốc độ tăng chiều cao chậm, cần chú ý chế độ dinh dưỡng và vận động");
            }

            if (assessment.GrowthTrend.WeightVelocity > 1M && assessment.ZScoreBMI > 1)
            {
                recommendations.Add("- Tốc độ tăng cân nhanh, cần điều chỉnh chế độ ăn để tránh thừa cân");
            }

            // Thêm các khuyến nghị chi tiết khác dựa trên tình trạng hiện tại
            if (assessment.ZScoreHeight < -2)
            {
                recommendations.Add("- Cần bổ sung vitamin D và canxi");
                recommendations.Add("- Đảm bảo chế độ ăn đủ protein");
                recommendations.Add("- Tăng cường vận động ngoài trời");
            }

            return string.Join("\n", recommendations);
        }
    }

   
}
