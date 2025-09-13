using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyBackend.Services;         
using MyBackend.ORCLEUPDATER;    

var builder = WebApplication.CreateBuilder(args);

// 添加控制器支持
builder.Services.AddControllers();


// 允许跨域
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// 注册服务
builder.Services.AddScoped<RecommendService>();
builder.Services.AddScoped<TestDataService>();

// ✅ 添加 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();

// ✅ 启用 Swagger UI（仅开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用路由到控制器
app.MapControllers();

app.Run();
