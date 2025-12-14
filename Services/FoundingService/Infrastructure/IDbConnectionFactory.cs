/*
重构代码：
*/
using System.Data;

namespace MyBackend.Infrastructure;

public interface IDbConnectionFactory
{
    /// <summary>
    /// 创建一个新的数据库连接（此时连接通常处于 Closed 状态）
    /// </summary>
    /// <returns>抽象的 IDbConnection</returns>
    IDbConnection CreateConnection();
}