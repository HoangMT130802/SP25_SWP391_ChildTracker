using AutoMapper;
using BusinessLogic.DTOs.Appointment;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.DTOs.Children;
using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.DTOs.GrowthRecord;
using BusinessLogic.DTOs.User;
using DataAccess.Entities;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BusinessLogic.Mappers
{
    // Custom JsonConverter cho DateOnly
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString()!, Format);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }

    // Custom JsonConverter cho TimeOnly
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeOnly.ParseExact(reader.GetString()!, Format);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }

    // Custom JsonConverter cho decimal
    public class DecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (decimal.TryParse(stringValue, out decimal value))
                {
                    return value;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            throw new JsonException("Unable to convert value to decimal");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Base User mapping configuration
            CreateMap<User, BaseUserDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status));

            // User mappings
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // Authentication mappings
            CreateMap<User, UserResponseDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status));
                

            CreateMap<RegisterRequestDTO, User>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true));

            // Doctor mappings
            CreateMap<User, DoctorDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
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

            // Child and Growth Record mappings
            CreateMap<Child, ChildDTO>();
            CreateMap<CreateChildDTO, Child>();
            CreateMap<UpdateChildDTO, Child>();

            CreateMap<GrowthRecord, GrowthRecordDTO>();
            CreateMap<CreateGrowthRecordDTO, GrowthRecord>();
            CreateMap<UpdateGrowthRecordDTO, GrowthRecord>();

            // Consultation mappings
            CreateMap<ConsultationRequest, ConsultationRequestDTO>();
            CreateMap<CreateConsultationRequestDTO, ConsultationRequest>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastActivityAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsSatisfied, opt => opt.MapFrom(src => false));

            CreateMap<ConsultationResponse, ConsultationResponseDTO>();
            CreateMap<CreateConsultationResponseDTO, ConsultationResponse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Appointment mappings
            CreateMap<Appointment, AppointmentDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.ChildName, opt => opt.MapFrom(src => src.Child.FullName))
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.Schedule.DoctorId))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Schedule.Doctor.FullName))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.Schedule.WorkDate));

            CreateMap<CreateAppointmentDTO, Appointment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}