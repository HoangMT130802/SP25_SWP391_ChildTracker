using AutoMapper;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.DTOs.Doctor_Schedule;
using DataAccess.Entities;

namespace BusinessLogic.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Auth mappings
            CreateMap<RegisterRequestDTO, User>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<User, UserResponseDTO>();

            // Doctor Schedule mappings
            CreateMap<DoctorSchedule, DoctorScheduleDTO>()
                .ForMember(dest => dest.DoctorName, 
                    opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.FullName : string.Empty))
                .ForMember(dest => dest.DoctorSpecialization,
                    opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Role : string.Empty))
                .ForMember(dest => dest.AvailableSlots,
                    opt => opt.Ignore())
                .ForMember(dest => dest.SelectedSlotIds,
                    opt => opt.Ignore());
        }
    }
} 