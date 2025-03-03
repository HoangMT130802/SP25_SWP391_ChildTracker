using AutoMapper;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.DTOs.Children;
using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.DTOs.GrowthRecord;
using DataAccess.Entities;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<User, UserResponseDTO>();

        CreateMap<RegisterRequestDTO, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username != null ? src.Username.Trim() : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Trim().ToLower() : null))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName != null ? src.FullName.Trim() : null))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Trim() : null))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address != null ? src.Address.Trim() : null))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        //Child mapping
        CreateMap<Child, ChildDTO>();
        CreateMap<CreateChildDTO, Child>();
        CreateMap<UpdateChildDTO, Child>();

        // GrowthRecord mappings
        CreateMap<GrowthRecord, GrowthRecordDTO>();
        CreateMap<CreateGrowthRecordDTO, GrowthRecord>();
        CreateMap<UpdateGrowthRecordDTO, GrowthRecord>();
        // mapping from user to doctor DTO
        CreateMap<User, DoctorDTO>()
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
             .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
             .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
             .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
             .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
             // Ánh xạ các thuộc tính từ DoctorProfile
             .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().Specialization
                     : null))
             .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().Qualification
                     : null))
             .ForMember(dest => dest.Experience, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().Experience
                     : 0))
             .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().LicenseNumber
                     : null))
             .ForMember(dest => dest.Biography, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().Biography
                     : null))
             .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().AverageRating
                     : 0))
             .ForMember(dest => dest.TotalRatings, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().TotalRatings
                     : 0))
             .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src =>
                 src.DoctorProfiles != null && src.DoctorProfiles.Any()
                     ? src.DoctorProfiles.FirstOrDefault().IsVerified
                     : false));
        //Map growth record để sử dụng trong assessment controller
        CreateMap<GrowthRecordDTO, GrowthRecord>();
        CreateMap<GrowthRecord, GrowthRecordDTO>();
        //Consulation
        CreateMap<ConsultationRequest, ConsultationRequestDTO>();
        CreateMap<ConsultationResponse, ConsultationResponseDTO>();
        CreateMap<CreateConsultationRequestDTO, ConsultationRequest>();
        CreateMap<CreateConsultationResponseDTO, ConsultationResponse>();
    }
}