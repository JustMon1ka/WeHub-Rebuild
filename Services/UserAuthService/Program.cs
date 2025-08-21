using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UserAuthService.Data;
using UserAuthService.Repositories;
using UserAuthService.Services;
using UserAuthService.Services.Interfaces;

namespace UserAuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------------------ DB ------------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ------------------ JWT ------------------
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddSingleton<JwtService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();


            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        ))
                    };
                });

            // ------------------ CORS ------------------
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // 允许前端端口
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // ------------------ Swagger ------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] { }
                    }
                });
            });

            // ------------------ Controllers & Repositories ------------------
            builder.Services.AddControllers();
            builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // ------------------ Dev Exception Page ------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // ------------------ Middleware Pipeline ------------------
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            app.UseCors(); // 启用 CORS，必须在 UseRouting 和 UseAuthorization 之间
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
