using BusinessLogic.DTOs.GrowthAssessment;
using BusinessLogic.DTOs.GrowthRecord;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IGrowthAssessmentService
    {
        Task<GrowthAssessmentDTO> AssessGrowthAsync(GrowthRecord record);
        decimal CalculateZScore(decimal value, decimal median, decimal sd);
        string GetNutritionalStatus(decimal zScore, string measurementType);
        string GetDetailedRecommendations(GrowthAssessmentDTO assessment);
    }
}
