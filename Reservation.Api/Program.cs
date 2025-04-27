using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Reservation.Api.CustomException;
using Reservation.Api.JWT;
using Reservation.Api.Models;
using Reservation.Api.Services;
using Reservation.Api.Workers;

namespace Reservation.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers(options => { options.Filters.Add(new AuthorizeFilter()); });

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter '<your-access-token>' in the box below."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.MapType<TimeOnly>(() => new OpenApiSchema
            {
                Type = "TimeOnly",
                Example = new OpenApiString(DateTime.UtcNow.ToString("HH:mm:ss")),
            });

            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "DateOnly",
                Example = new OpenApiString(DateTime.Today.ToString("yyyy-MM-dd")),
            });
            options.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = "TimeSpan",
                Example = new OpenApiString("TimeSpan"),
            });
        });

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddSingleton<JwtTokenHelper>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddHostedService<RefreshTokenCleanupWorker>();
        builder.Services.AddHostedService<ReservationReminderWorker>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


        builder.Services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.SetPostgresVersion(14, 0)));

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new
                            Exception("JWT key is missing"))),
                };
            });

        builder.Services.AddCors(options => options.AddPolicy(
            "wasm",
            policy => policy.WithOrigins([
                    builder.Configuration["BackendUrl"] ??
                    throw new InvalidOperationException("Chybí proměnná prostředí BackendUrl"),
                    builder.Configuration["FrontendUrl"] ??
                    throw new InvalidOperationException("Chybí proměnná prostředí FrontendUrl")
                ])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()));

        var app = builder.Build();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("wasm");

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}