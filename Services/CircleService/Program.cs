using CircleService.Data;
using CircleService.Repositories;
using CircleService.Services;
using CircleService.Services.JoinHandlers;
using CircleService.Services.Decorators;
using CircleService.Builders;
using CircleService.Facades;
using CircleService.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------ 依赖注入容器 (DI Container) ------------------

// 1. 注册数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. 注册 Services 和 Repositories
builder.Services.AddScoped<ICircleRepository, CircleRepository>();
builder.Services.AddScoped<ICircleService>(sp =>
{
    // 原本真正的 CircleService
    var real = new CircleService(
        sp.GetRequiredService<ICircleRepository>(),
        sp.GetRequiredService<IUserRepository>(),
        sp.GetRequiredService<IFileService>(),
        sp.GetRequiredService<CircleBuilder>(),
        sp.GetRequiredService<CircleCreationFacade>()
    );

    // 加日志
    var logger = sp.GetRequiredService<ILogger<LoggingCircleServiceDecorator>>();
    var logging = new LoggingCircleServiceDecorator(real, logger);

    // 加计时
    var timed = new TimingCircleServiceDecorator(logging);

    return timed;
});

builder.Services.AddScoped<ICircleMemberRepository, CircleMemberRepository>();
builder.Services.AddScoped<ICircleMemberService, CircleMemberService>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IActivityParticipantRepository, ActivityParticipantRepository>();
builder.Services.AddScoped<IActivityParticipantService, ActivityParticipantService>();
builder.Services.AddScoped<IActivityAutoCompleteService, ActivityAutoCompleteService>();
builder.Services.AddScoped<ICircleCreationFacade, CircleCreationFacade>();
builder.Services.AddScoped<JoinHandlerFactory>();
builder.Services.AddSingleton<ICircleSubject, CircleEventSubject>();

builder.Services.AddSingleton<ICircleObserver, LoggingCircleObserver>();
builder.Services.AddSingleton<ICircleObserver, RecommendationCircleObserver>();

builder.Services.AddSingleton(sp =>
{
    var subject = sp.GetRequiredService<ICircleSubject>();

    foreach (var obs in sp.GetServices<ICircleObserver>())
        subject.Register(obs);

    return subject; // 让 DI 完成 observer 绑定
});




// 3. 配置 FileBrowser 服务
builder.Services.Configure<FileBrowserOptions>(builder.Configuration.GetSection("FileBrowser"));
builder.Services.AddScoped<IFileBrowserClient, FileBrowserClient>();

// 4. 配置 JWT 认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

// 5. 添加授权服务
builder.Services.AddAuthorization();

// 6. 添加控制器服务
builder.Services.AddControllers();

// 4. 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // 允许的前端地址
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// 7. 配置 Swagger (用于 API 文档和测试)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Circle Service API", Version = "v1" });
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


// ------------------ 中间件管道 (Middleware Pipeline) ------------------

var app = builder.Build();

// 在开发环境中启用 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用 HTTPS 重定向
app.UseHttpsRedirection();

// 启用路由
app.UseRouting();

// 启用 CORS 策略
app.UseCors();

// 启用认证（必须在授权之前）
app.UseAuthentication();

// 启用授权
app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// 启动应用程序
app.Run();
