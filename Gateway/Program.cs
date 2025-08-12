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

            // 1. 从配置文件加载 YARP 的配置信息
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            var app = builder.Build();

            // 2. 启用 YARP 反向代理中间件
            app.MapReverseProxy();

            app.Run();
        }
    }
}