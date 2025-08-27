using CircleService.Data;
using CircleService.Repositories;
using CircleService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------ 依赖注入容器 (DI Container) ------------------

// 1. 注册数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. 注册 Services 和 Repositories
builder.Services.AddScoped<ICircleRepository, CircleRepository>();
builder.Services.AddScoped<ICircleService, CircleService.Services.CircleService>();
builder.Services.AddScoped<ICircleMemberRepository, CircleMemberRepository>();
builder.Services.AddScoped<ICircleMemberService, CircleMemberService>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IActivityParticipantRepository, ActivityParticipantRepository>();
builder.Services.AddScoped<IActivityParticipantService, ActivityParticipantService>();

// 3. 配置 FileBrowser 服务
builder.Services.Configure<FileBrowserOptions>(builder.Configuration.GetSection("FileBrowser"));
builder.Services.AddScoped<IFileBrowserClient, FileBrowserClient>();


// 3. 添加控制器服务
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


// 5. 配置 Swagger (用于 API 文档和测试)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

// 启用授权
app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// 启动应用程序
app.Run();
