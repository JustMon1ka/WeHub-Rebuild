using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly IConfiguration _config;

    public TopicsController(IConfiguration config)
    {
        _config = config;
    }

    // GET: /api/topics/{tagName}
    [HttpGet("{tagName}")]
    public IActionResult GetTopicCount(string tagName)
    {
        var connStr = _config.GetConnectionString("DefaultConnection");

        try
        {
            using var conn = new OracleConnection(connStr);
            conn.Open();

            string sql = @"
                SELECT COUNT(p.POST_ID)
                FROM POST p
                JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
                JOIN TAG t ON pt.TAG_ID = t.TAG_ID
                WHERE t.TAG_NAME = :tagName
                  AND NVL(p.IS_DELETED,0)=0
                  AND NVL(p.IS_HIDDEN,0)=0";

            using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("tagName", tagName));

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new { topic = tagName, count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
