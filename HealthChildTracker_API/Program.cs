using BusinessLogic.Services.Interfaces;
using BusinessLogic.Services.Implementations;
using DataAccess.UnitOfWork;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//đăng ký db
builder.Services.AddDbContext<HealthChildTrackerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// đăng ký các service
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IGrowthRecordService, GrowthRecordService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
// Đăng ký automapper
builder.Services.AddAutoMapper(typeof(MapperProfile));
// cấu hình cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder
                .WithOrigins("http://localhost:5174") // URL của React app
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();