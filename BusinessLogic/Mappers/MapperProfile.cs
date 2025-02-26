using AutoMapper;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.DTOs.Children;
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
    }
}