using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using PostService.Config;
using PostService.Data;
using PostService.Repositories;
using PostService.Services;
using StackExchange.Redis;

namespace PostService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // ------------------ Redis ------------------
            var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfig>();

            if (redisConfig == null || string.IsNullOrWhiteSpace(redisConfig.ConnectionString))
            {
                // 如果配置不存在或为空，可以抛出异常或记录警告
                Console.WriteLine("Redis ConnectionString is not configured.");
                // 或者直接返回，避免应用启动失败
                return;
            }

            // 将 Redis 连接配置为单例服务
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConfig.ConnectionString));

            // ------------------ DB ------------------
            // builder.Services.AddDbContext<AppDbContext>(options =>
            //     options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 注册 DbContext 工厂，使它能够被多次创建
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ------------------ JWT ------------------
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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

            builder.Services.AddAuthorization();

            // ------------------ CORS ------------------
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:5173",// 前端地址
                            "http://localhost:5000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // ------------------ Swagger ------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "User Data API", Version = "v1" });
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

            // ------------------ Controllers & Services ------------------
            builder.Services.AddControllers();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IPostRedisRepository, PostRedisRepository>();
            // 注册 HttpClient 用于调用 NoticeService
            builder.Services.AddHttpClient<ILikeService, LikeService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IPostService, PostService.Services.PostService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ILikeRepository, LikeRepository>();
            builder.Services.AddScoped<IShareService, ShareService>();
            builder.Services.AddScoped<FavoriteRepository>();
            builder.Services.AddScoped<FavoriteService>();

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
            app.UseCors(); // 必须在 routing 和 auth 之间
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
