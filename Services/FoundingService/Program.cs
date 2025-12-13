/*
原有代码：
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
*/
/*
现有代码：
*/
using MyBackend.Facades;
using MyBackend.Facades.Interfaces;
using MyBackend.Infrastructure;
using MyBackend.ORCLEUPDATER; // 保留原有的测试数据工具
using MyBackend.Services;
using MyBackend.Strategies;
using MyBackend.Strategies.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. 注册基础服务 (Framework Services)
// ==========================================

builder.Services.AddControllers();

// 配置 CORS (跨域)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 配置 Swagger API 文档
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ==========================================
// 2. 注册自定义服务 (Application Services)
// ==========================================

// --- Infrastructure (基础设施层) ---
// 数据库连接工厂：无状态，全应用只需一个实例，所以用 Singleton
builder.Services.AddSingleton<IDbConnectionFactory, OracleConnectionFactory>();


// --- Strategies (策略层) ---
// 相似度算法：纯数学计算，无状态，用 Singleton
builder.Services.AddSingleton<ISimilarityStrategy, CosineSimilarityStrategy>();


// --- Facades (外观层/数据层) ---
// 封装了数据库连接，生命周期建议跟随 HTTP 请求，所以用 Scoped
builder.Services.AddScoped<IRecommendDbFacade, RecommendDbFacade>();
builder.Services.AddScoped<ITopicDbFacade, TopicDbFacade>();
builder.Services.AddScoped<IDbMetaFacade, DbMetaFacade>();


// --- Services (业务层) ---
// 业务逻辑，依赖于 Scoped 的 Facades，所以也必须是 Scoped
builder.Services.AddScoped<RecommendService>();
builder.Services.AddScoped<TopicService>();
builder.Services.AddScoped<DatabaseToolService>();

// (保留原有) 测试数据生成服务
builder.Services.AddScoped<TestDataService>();


// ==========================================
// 3. 构建与管道配置 (Pipeline Configuration)
// ==========================================

var app = builder.Build();

app.UseCors();

// 仅在开发环境下启用 Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();