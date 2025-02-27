using AutoMapper;
using BusinessLogic.DTOs.Doctor_Schedule;
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
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorScheduleService> _logger;

        public DoctorScheduleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DoctorScheduleService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<DoctorScheduleDTO>> GetAllSchedulesAsync()
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedules = await scheduleRepository.GetAllAsync("Doctor");
                return _mapper.Map<IEnumerable<DoctorScheduleDTO>>(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all schedules");
                throw;
            }
        }
    }
}
