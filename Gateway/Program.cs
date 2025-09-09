using Yarp.ReverseProxy.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 启用全局 CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // 加载 YARP 配置
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            var app = builder.Build();

            // 使用 CORS 中间件（必须在 MapReverseProxy 之前）
            app.UseCors("AllowAll");

            // 启用 YARP 反向代理
            app.MapReverseProxy();

            app.Run();
        }
    }
}