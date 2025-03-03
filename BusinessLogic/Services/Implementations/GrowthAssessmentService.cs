using AutoMapper;
using BusinessLogic.DTOs.GrowthAssessment;
using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class GrowthAssessmentService : IGrowthAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GrowthAssessmentService> _logger;
        private readonly IMapper _mapper;

        private const decimal DAYS_PER_MONTH = 30.44M;
        private const decimal CONCERNING_HEIGHT_VELOCITY = 0.5M;
        private const decimal CONCERNING_WEIGHT_VELOCITY = 0.1M;
        private const decimal CONCERNING_BMI_VELOCITY = 0.5M;

        private class MeasurementData
        {
            public decimal Value { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public GrowthAssessmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GrowthAssessmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GrowthAssessmentDTO> AssessGrowthAsync(GrowthRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            // Kiểm tra các giá trị đo lường
            if (record.Height <= 0 || record.Weight <= 0)
                throw new ArgumentException("Chiều cao và cân nặng phải lớn hơn 0");
            try
            {
                // Lấy thông tin trẻ
                var childRepo = _unitOfWork.GetRepository<Child>();
                var child = await childRepo.GetAsync(c => c.ChildId == record.ChildId);

                if (child == null)
                    throw new KeyNotFoundException($"Không tìm thấy trẻ với ID {record.ChildId}");

                // Tính tuổi chính xác tại thời điểm đo
                decimal exactAgeInMonths = CalculateExactAgeInMonths(child.BirthDate, record.CreatedAt);

                // Lấy và nội suy các chỉ số chuẩn
                var standards = await GetInterpolatedStandards(
                    child.Gender,
                    exactAgeInMonths
                );

                var assessment = new GrowthAssessmentDTO
                {
                    RecordId = record.RecordId,
                    ChildId = record.ChildId,
                    ExactAgeInMonths = exactAgeInMonths,
                    MeasurementDate = record.CreatedAt,

                    // Các chỉ số đo được
                    Height = record.Height,
                    Weight = record.Weight,
                    BMI = record.Bmi,
                    HeadCircumference = record.HeadCircumference,

                    // Tính Z-score cho từng chỉ số
                    ZScores = new GrowthZScoresDTO
                    {
                        Height = CalculateZScore(
                            record.Height,
                            standards.Height.Median,
                            standards.Height.Sd1pos - standards.Height.Median
                        ),

                        Weight = CalculateZScore(
                            record.Weight,
                            standards.Weight.Median,
                            standards.Weight.Sd1pos - standards.Weight.Median
                        ),

                        BMI = CalculateZScore(
                            record.Bmi,
                            standards.BMI.Median,
                            standards.BMI.Sd1pos - standards.BMI.Median
                        ),

                        HeadCircumference = CalculateZScore(
                            record.HeadCircumference,
                            standards.HeadCircumference.Median,
                            standards.HeadCircumference.Sd1pos - standards.HeadCircumference.Median
                        )
                    }
                };

                // Phân tích xu hướng tăng trưởng
                var growthHistory = await GetGrowthHistory(record.ChildId, record.CreatedAt);
                assessment.GrowthTrend = AnalyzeGrowthTrend(growthHistory);

                // Đánh giá tình trạng
                assessment.Assessments = new GrowthAssessmentsDTO
                {
                    HeightStatus = GetNutritionalStatus(assessment.ZScores.Height, "Height"),
                    WeightStatus = GetNutritionalStatus(assessment.ZScores.Weight, "Weight"),
                    BMIStatus = GetNutritionalStatus(assessment.ZScores.BMI, "BMI"),
                    HeadCircumferenceStatus = GetNutritionalStatus(
                        assessment.ZScores.HeadCircumference,
                        "HeadCircumference"
                    )
                };

                // Đưa ra khuyến nghị
                assessment.Recommendations = GetDetailedRecommendations(assessment);

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi đánh giá tăng trưởng cho trẻ {record.ChildId}");
                throw;
            }
        }
        public decimal CalculateZScore(decimal value, decimal median, decimal sd)
        {
            return (value - median) / sd;
        }
        public string GetNutritionalStatus(decimal zScore, string measurementType)
        {
            switch (measurementType)
            {
                case "Height":
                    if (zScore < -3) return "Thấp còi nghiêm trọng";
                    if (zScore < -2) return "Thấp còi";
                    if (zScore < -1) return "Chiều cao thấp";
                    if (zScore <= 2) return "Chiều cao bình thường";
                    return "Chiều cao cao";

                case "Weight":
                    if (zScore < -3) return "Suy dinh dưỡng nặng";
                    if (zScore < -2) return "Suy dinh dưỡng";
                    if (zScore <= 1) return "Cân nặng bình thường";
                    if (zScore <= 2) return "Thừa cân";
                    return "Béo phì";

                case "BMI":
                    if (zScore < -3) return "Gầy độ 3";
                    if (zScore < -2) return "Gầy độ 2";
                    if (zScore < -1) return "Gầy độ 1";
                    if (zScore <= 1) return "BMI bình thường";
                    if (zScore <= 2) return "Thừa cân";
                    if (zScore <= 3) return "Béo phì độ 1";
                    return "Béo phì độ 2";

                case "HeadCircumference":
                    if (zScore < -2) return "Vòng đầu nhỏ";
                    if (zScore <= 2) return "Vòng đầu bình thường";
                    return "Vòng đầu lớn";

                default:
                    return "Không xác định";
            }
        }
        private decimal CalculateExactAgeInMonths(DateTime birthDate, DateTime measurementDate)
        {
            var timeSpan = measurementDate - birthDate;
            return (decimal)timeSpan.TotalDays / DAYS_PER_MONTH;
        }

        private decimal InterpolateValue(decimal start, decimal end, decimal fraction)
        {
            return start + (end - start) * fraction;
        }

        private bool IsGrowthConcerning(GrowthTrendDTO trend)
        {
            if (trend.HeightVelocity < CONCERNING_HEIGHT_VELOCITY) return true;
            if (trend.WeightVelocity < CONCERNING_WEIGHT_VELOCITY) return true;
            if (trend.WeightVelocity > 1M && trend.BMIVelocity > CONCERNING_BMI_VELOCITY) return true;
            return false;
        }

     /*   private decimal CalculateVelocity<T>(List<T> measurements) where T : class
        {
            if (measurements.Count < 2) return 0;

            dynamic firstMeasurement = measurements.First();
            dynamic lastMeasurement = measurements.Last();
            var monthsDifference = (lastMeasurement.CreatedAt - firstMeasurement.CreatedAt).TotalDays / DAYS_PER_MONTH;

            if (monthsDifference == 0) return 0;

            decimal firstValue = firstMeasurement.Value;
            decimal lastValue = lastMeasurement.Value;
            return (lastValue - firstValue) / monthsDifference;
        }
*/
        private async Task<GrowthStandardsDTO> GetInterpolatedStandards(string gender, decimal exactAgeInMonths)
        {
            var standardRepo = _unitOfWork.GetRepository<GrowthStandard>();

            // Lấy các tiêu chuẩn gần nhất
            var lowerAge = (int)Math.Floor(exactAgeInMonths);
            var upperAge = (int)Math.Ceiling(exactAgeInMonths);
            var fraction = exactAgeInMonths - lowerAge;

            var standards = new GrowthStandardsDTO();
            var measurements = new[] { "Height", "Weight", "BMI", "HeadCircumference" };

            foreach (var measurement in measurements)
            {
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

                var interpolatedStandard = new GrowthStandard
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

                switch (measurement)
                {
                    case "Height":
                        standards.Height = interpolatedStandard;
                        break;
                    case "Weight":
                        standards.Weight = interpolatedStandard;
                        break;
                    case "BMI":
                        standards.BMI = interpolatedStandard;
                        break;
                    case "HeadCircumference":
                        standards.HeadCircumference = interpolatedStandard;
                        break;
                }
            }

            return standards;
        }

        private async Task<List<GrowthRecord>> GetGrowthHistory(int childId, DateTime currentDate)
        {
            var recordRepo = _unitOfWork.GetRepository<GrowthRecord>();
            var records = await recordRepo.FindAsync(
                r => r.ChildId == childId &&
                     r.CreatedAt < currentDate &&
                     r.CreatedAt >= currentDate.AddMonths(-6),
                includeProperties: "Child"
            );

            return records.OrderBy(r => r.CreatedAt).ToList();
        }

        private GrowthTrendDTO AnalyzeGrowthTrend(List<GrowthRecord> history)
        {
            if (!history.Any())
                return new GrowthTrendDTO { HasSufficientData = false };

            var heightData = history.Select(h => new MeasurementData { Value = h.Height, CreatedAt = h.CreatedAt }).ToList();
            var weightData = history.Select(h => new MeasurementData { Value = h.Weight, CreatedAt = h.CreatedAt }).ToList();
            var bmiData = history.Select(h => new MeasurementData { Value = h.Bmi, CreatedAt = h.CreatedAt }).ToList();

            var trend = new GrowthTrendDTO
            {
                HasSufficientData = true,
                HeightVelocity = CalculateVelocity(heightData),
                WeightVelocity = CalculateVelocity(weightData),
                BMIVelocity = CalculateVelocity(bmiData),
                LastMeasurementDate = history.Max(h => h.CreatedAt),
                NumberOfMeasurements = history.Count
            };

            trend.IsGrowthConcerning = IsGrowthConcerning(trend);

            return trend;
        }



        private decimal CalculateVelocity(List<MeasurementData> measurements)
        {
            if (measurements.Count < 2) return 0;

            var firstMeasurement = measurements.First();
            var lastMeasurement = measurements.Last();

            // Chuyển đổi TimeSpan thành decimal trước khi chia
            decimal daysDifference = (decimal)(lastMeasurement.CreatedAt - firstMeasurement.CreatedAt).TotalDays;
            decimal monthsDifference = daysDifference / DAYS_PER_MONTH;

            if (monthsDifference == 0) return 0;

            return (lastMeasurement.Value - firstMeasurement.Value) / monthsDifference;
        }

        public string GetDetailedRecommendations(GrowthAssessmentDTO assessment)
        {
            var recommendations = new List<string>();

            // Đánh giá chiều cao
            if (assessment.ZScores.Height < -3)
            {
                recommendations.Add("⚠️ Trẻ đang bị thấp còi nghiêm trọng (độ III):");
                recommendations.Add("- Cần đưa trẻ đi khám bác sĩ chuyên khoa nhi gấp");
                recommendations.Add("- Cần kiểm tra các vấn đề về nội tiết và dinh dưỡng");
            }
            else if (assessment.ZScores.Height < -2)
            {
                recommendations.Add("⚠️ Trẻ đang bị thấp còi (độ II):");
                recommendations.Add("- Cần bổ sung vitamin D và canxi");
                recommendations.Add("- Đảm bảo chế độ ăn đủ protein (thịt, cá, trứng, sữa)");
                recommendations.Add("- Tăng cường vận động ngoài trời");
            }

            // Đánh giá cân nặng và BMI
            if (assessment.ZScores.BMI > 3)
            {
                recommendations.Add("⚠️ Trẻ đang bị béo phì độ II:");
                recommendations.Add("- Cần tham vấn bác sĩ về chế độ ăn phù hợp");
                recommendations.Add("- Giảm thức ăn nhiều đường và chất béo");
                recommendations.Add("- Tăng cường vận động thể chất mỗi ngày");
            }
            else if (assessment.ZScores.BMI > 2)
            {
                recommendations.Add("⚠️ Trẻ đang bị béo phì độ I:");
                recommendations.Add("- Điều chỉnh chế độ ăn uống hợp lý");
                recommendations.Add("- Tăng cường hoạt động thể chất");
            }

            // Đánh giá xu hướng tăng trưởng
            if (assessment.GrowthTrend.HasSufficientData)
            {
                if (assessment.GrowthTrend.HeightVelocity < CONCERNING_HEIGHT_VELOCITY)
                {
                    recommendations.Add("📊 Tốc độ tăng chiều cao đang chậm:");
                    recommendations.Add("- Cần theo dõi sát sao hơn");
                    recommendations.Add("- Đảm bảo trẻ ngủ đủ giấc");
                }
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("✅ Trẻ đang phát triển bình thường.");
                recommendations.Add("- Tiếp tục duy trì chế độ dinh dưỡng và vận động hiện tại");
            }

            return string.Join("\n", recommendations);
        }
    }
}
