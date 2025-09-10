using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NoticeService.Data;
using NoticeService.Repositories;
using NoticeService.Services;
using Oracle.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));

// Oracle ≈‰÷√
builder.Services.AddDbContext<NoticeDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis ≈‰÷√
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    return ConnectionMultiplexer.Connect(redisConfig);
});

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();