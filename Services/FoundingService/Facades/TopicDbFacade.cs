/*
重构代码：refactoring with Facade Pattern
*/
using System.Data;
using MyBackend.Facades.Interfaces;
using MyBackend.Infrastructure;
using Oracle.ManagedDataAccess.Client;

namespace MyBackend.Facades;

public class TopicDbFacade : ITopicDbFacade
{
    private readonly IDbConnectionFactory _dbFactory;

    public TopicDbFacade(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<int> FetchTopicPostCountAsync(string tagName)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();

        string sql = @"
            SELECT COUNT(p.POST_ID)
            FROM POST p
            JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
            JOIN TAG t ON pt.TAG_ID = t.TAG_ID
            WHERE t.TAG_NAME = :tagName
              AND NVL(p.IS_DELETED,0)=0
              AND NVL(p.IS_HIDDEN,0)=0";

        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new OracleParameter("tagName", tagName));

        // ExecuteScalar 返回 object，需要转换
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}