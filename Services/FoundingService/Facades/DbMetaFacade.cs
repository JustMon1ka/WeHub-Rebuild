/*
重构代码：
refactoring with Facade Pattern+Data Transfer Object
*/
using System.Data;
using MyBackend.Facades.Interfaces;
using MyBackend.Infrastructure;
using MyBackend.Models;
using Oracle.ManagedDataAccess.Client;

namespace MyBackend.Facades;

public class DbMetaFacade : IDbMetaFacade
{
    private readonly IDbConnectionFactory _dbFactory;

    public DbMetaFacade(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    private IDbConnection GetConnection()
    {
        var conn = _dbFactory.CreateConnection();
        conn.Open();
        return conn;
    }

    public async Task<IEnumerable<string>> FetchAllTableNamesAsync()
    {
        var tables = new List<string>();
        using var conn = GetConnection();

        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = "SELECT table_name FROM user_tables";
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public async Task<IEnumerable<TableMetaResult>> FetchTableColumnsAsync(string tableName)
    {
        var columns = new List<TableMetaResult>();
        using var conn = GetConnection();

        string sql = @"
            SELECT column_name, data_type, data_length, nullable
            FROM user_tab_columns
            WHERE table_name = :tableName
            ORDER BY column_id";

        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new OracleParameter("tableName", tableName.ToUpper()));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new TableMetaResult
            {
                ColumnName = reader.GetString(0),
                DataType = reader.GetString(1),
                Length = reader.GetInt32(2),
                Nullable = reader.GetString(3)
            });
        }
        return columns;
    }

    public async Task<IEnumerable<object>> FetchAllTagsAsync()
    {
        var tags = new List<object>();
        using var conn = GetConnection();

        string sql = "SELECT TAG_ID, TAG_NAME, COUNT, LAST_QUOTE FROM TAG ORDER BY TAG_ID";
        
        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            // 这里因为结构比较松散，可以使用匿名对象或字典
            // 为了兼容原代码的 json 格式
            tags.Add(new {
                tag_id = reader.GetInt32(0),
                tag_name = reader.GetString(1),
                count = reader.GetInt32(2),
                last_quote = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
            });
        }
        return tags;
    }
}