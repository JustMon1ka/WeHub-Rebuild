using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace MyBackend.Controllers;

[ApiController]
[Route("dbtest")]
public class DbTestController : ControllerBase
{
    private readonly IConfiguration _config;
    public DbTestController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("tables")]
    public IActionResult GetTables()
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        var tables = new List<string>();

        try
        {
            using var conn = new OracleConnection(connStr);
            conn.Open();

            // 查询当前用户下的所有表
            using var cmd = new OracleCommand("SELECT table_name FROM user_tables", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return Ok(new { tables });
    }


    [HttpGet("columns")]
    public IActionResult GetColumns([FromQuery] string table)
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        var columns = new List<object>();

        try
        {
            using var conn = new OracleConnection(connStr);
            conn.Open();

            string sql = @"SELECT column_name, data_type, data_length, nullable
                        FROM user_tab_columns
                        WHERE table_name = :tableName
                        ORDER BY column_id";

            using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("tableName", table.ToUpper()));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(new {
                    ColumnName = reader.GetString(0),
                    DataType   = reader.GetString(1),
                    Length     = reader.GetInt32(2),
                    Nullable   = reader.GetString(3)
                });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return Ok(new { table, columns });
    }


   [HttpGet("tag")]
    public IActionResult GetTags()
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        var tags = new List<object>();

        try
        {
            using var conn = new OracleConnection(connStr);
            conn.Open();

            string sql = "SELECT TAG_ID, TAG_NAME, COUNT, LAST_QUOTE FROM TAG ORDER BY TAG_ID";
            using var cmd = new OracleCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(new {
                    tag_id = reader.GetInt32(0),
                    tag_name = reader.GetString(1),
                    count = reader.GetInt32(2),
                    last_quote = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
                });

            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return Ok(new { tags });
    }

}
