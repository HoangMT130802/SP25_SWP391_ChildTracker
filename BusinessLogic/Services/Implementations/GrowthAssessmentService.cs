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
                // Lấy thông tin trẻ để biết tuổi và giới tính
                var childRepo = _unitOfWork.GetRepository<Child>();
                var child = await childRepo.GetAsync(c => c.ChildId == childId);

                if (child == null)
                    throw new KeyNotFoundException($"Không tìm thấy trẻ với ID {childId}");

                // Tính tuổi theo tháng
                int ageInMonths = CalculateAgeInMonths(child.BirthDate);

                // Lấy các chỉ số chuẩn theo tuổi và giới tính
                var standardRepo = _unitOfWork.GetRepository<GrowthStandard>();
                var heightStandard = await standardRepo.GetAsync(s =>
                    s.Gender == child.Gender &&
                    s.AgeInMonths == ageInMonths &&
                    s.Measurement == "Height");

                var weightStandard = await standardRepo.GetAsync(s =>
                    s.Gender == child.Gender &&
                    s.AgeInMonths == ageInMonths &&
                    s.Measurement == "Weight");

                var bmiStandard = await standardRepo.GetAsync(s =>
                    s.Gender == child.Gender &&
                    s.AgeInMonths == ageInMonths &&
                    s.Measurement == "BMI");

                var headCircumferenceStandard = await standardRepo.GetAsync(s =>
                    s.Gender == child.Gender &&
                    s.AgeInMonths == ageInMonths &&
                    s.Measurement == "HeadCircumference");

                var assessment = new GrowthAssessmentDTO();

                // Tính Z-score cho từng chỉ số
                assessment.ZScoreHeight = CalculateZScore(record.Height,
                    heightStandard.Median,
                    (heightStandard.Sd1pos - heightStandard.Median));

                assessment.ZScoreWeight = CalculateZScore(record.Weight,
                    weightStandard.Median,
                    (weightStandard.Sd1pos - weightStandard.Median));

                assessment.ZScoreBMI = CalculateZScore(record.Bmi,
                    bmiStandard.Median,
                    (bmiStandard.Sd1pos - bmiStandard.Median));

                assessment.ZScoreHeadCircumference = CalculateZScore(record.HeadCircumference,
                    headCircumferenceStandard.Median,
                    (headCircumferenceStandard.Sd1pos - headCircumferenceStandard.Median));

                // Đánh giá tình trạng
                assessment.HeightStatus = GetNutritionalStatus(assessment.ZScoreHeight, "Height");
                assessment.WeightStatus = GetNutritionalStatus(assessment.ZScoreWeight, "Weight");
                assessment.BMIStatus = GetNutritionalStatus(assessment.ZScoreBMI, "BMI");
                assessment.HeadCircumferenceStatus = GetNutritionalStatus(
                    assessment.ZScoreHeadCircumference, "HeadCircumference");

                // Đưa ra lời khuyên
                assessment.Recommendations = GetRecommendations(assessment);

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh giá tăng trưởng");
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
                    if (zScore < -3) return "Thấp còi độ 3";
                    if (zScore < -2) return "Thấp còi độ 2";
                    if (zScore < -1) return "Thấp còi độ 1";
                    if (zScore <= 2) return "Chiều cao bình thường";
                    return "Cao hơn bình thường";

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

                default:
                    return "Không xác định";
            }
        }

        public string GetRecommendations(GrowthAssessmentDTO assessment)
        {
            var recommendations = new List<string>();

            // Thêm lời khuyên dựa trên từng chỉ số
            if (assessment.ZScoreHeight < -2)
            {
                recommendations.Add("- Cần bổ sung dinh dưỡng và vitamin để cải thiện chiều cao");
                recommendations.Add("- Nên tham khảo ý kiến bác sĩ về kế hoạch dinh dưỡng");
            }

            if (assessment.ZScoreBMI > 2)
            {
                recommendations.Add("- Cần điều chỉnh chế độ ăn uống, giảm thức ăn nhiều năng lượng");
                recommendations.Add("- Tăng cường vận động thể chất");
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("Các chỉ số phát triển đang ở mức bình thường");
                recommendations.Add("Tiếp tục duy trì chế độ dinh dưỡng và vận động hiện tại");
            }

            return string.Join("\n", recommendations);
        }

        private int CalculateAgeInMonths(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            return ((today.Year - dateOfBirth.Year) * 12) + today.Month - dateOfBirth.Month;
        }
    }
}
