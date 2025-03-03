using AutoMapper;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.DTOs.Children;
using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.DTOs.User;
using DataAccess.Entities;
using System.Linq;

namespace BusinessLogic.Mappers
{
    public class MapperProfile : Profile
    {
        private string GetFirstName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return string.Empty;
            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private string GetLastName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return string.Empty;
            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
        }

        public MapperProfile()
        {
            // User mappings
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => GetFirstName(src.FullName)))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => GetLastName(src.FullName)))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<UpdateUserProfileDTO, User>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            // Doctor mappings
            CreateMap<User, DoctorDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => GetFirstName(src.FullName)))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => GetLastName(src.FullName)))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                // Map DoctorProfile properties
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? src.DoctorProfiles.First().Specialization 
                        : null))
                .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? src.DoctorProfiles.First().Qualification 
                        : null))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? src.DoctorProfiles.First().LicenseNumber 
                        : null))
                .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? src.DoctorProfiles.First().Experience.ToString() 
                        : "0"))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? src.DoctorProfiles.First().Biography 
                        : null))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? (double)src.DoctorProfiles.First().AverageRating 
                        : 0.0))
                .ForMember(dest => dest.ConsultationCount, opt => opt.MapFrom(src => 
                    src.DoctorProfiles != null && src.DoctorProfiles.Any() 
                        ? src.DoctorProfiles.First().TotalRatings 
                        : 0));

            CreateMap<CreateDoctorDTO, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Doctor"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            CreateMap<CreateDoctorDTO, DoctorProfile>()
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => src.Qualification))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalRatings, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => true));

            // Child mappings
            CreateMap<Child, ChildDTO>();
            CreateMap<CreateChildDTO, Child>();
            CreateMap<UpdateChildDTO, Child>();

            // GrowthRecord mappings
            CreateMap<GrowthRecord, GrowthRecordDTO>();
            CreateMap<CreateGrowthRecordDTO, GrowthRecord>();
            CreateMap<UpdateGrowthRecordDTO, GrowthRecord>();
            CreateMap<GrowthRecordDTO, GrowthRecord>();

            // Consultation mappings
            CreateMap<ConsultationRequest, ConsultationRequestDTO>();
            CreateMap<ConsultationResponse, ConsultationResponseDTO>();
            CreateMap<CreateConsultationRequestDTO, ConsultationRequest>();
            CreateMap<CreateConsultationResponseDTO, ConsultationResponse>();
        }
    }
}