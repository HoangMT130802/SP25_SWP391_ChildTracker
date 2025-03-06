using AutoMapper;
using BusinessLogic.DTOs.Doctor_Schedule;
using DataAccess.Entities;

namespace BusinessLogic.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ... existing mappings ...

            CreateMap<DoctorSchedule, DoctorScheduleDTO>()
                .ForMember(dest => dest.DoctorName, 
                    opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.FullName : string.Empty))
                .ForMember(dest => dest.DoctorSpecialization,
                    opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Role : string.Empty))
                .ForMember(dest => dest.AvailableSlots,
                    opt => opt.Ignore()) // This will be set manually
                .ForMember(dest => dest.SelectedSlotIds,
                    opt => opt.Ignore()); // This will be set manually
        }
    }
} 