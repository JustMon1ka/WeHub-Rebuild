/*
重构代码：
*/
using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace MyBackend.Infrastructure;

public class OracleConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(IConfiguration config)
    {
        // 从配置文件 appsettings.json 中读取连接字符串
        // 原代码中有 "DefaultConnection" 和 "OracleDb" 两种写法，
        // 建议在重构时统一为 "DefaultConnection"。
        _connectionString = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }
    }

    public IDbConnection CreateConnection()
    {
        // 返回具体的 OracleConnection，但在返回值中向上转型为 IDbConnection
        return new OracleConnection(_connectionString);
    }
}