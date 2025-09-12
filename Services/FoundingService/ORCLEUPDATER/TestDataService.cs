using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;

namespace MyBackend.ORCLEUPDATER;

public class TestDataService
{
    private readonly IConfiguration _config;
    public TestDataService(IConfiguration config) => _config = config;

    private OracleConnection GetConnection()
        => new OracleConnection(_config.GetConnectionString("OracleDb"));

    // 插入测试数据
    public void InsertTestData()
    {
        using var conn = GetConnection();
        conn.Open();

        using var tx = conn.BeginTransaction();
        try
        {
            // // 插入用户
            // ExecuteNonQuery(conn, "INSERT INTO USERS (USER_ID, USERNAME) VALUES (100295, 'Alice')");
            // ExecuteNonQuery(conn, "INSERT INTO USERS (USER_ID, USERNAME) VALUES (100296, 'Bob')");

            // // 插入标签
            // ExecuteNonQuery(conn, "INSERT INTO TAG (TAG_ID, TAG_NAME, COUNT, LAST_QUOTE) VALUES (360, 'AI_test', 0, SYSDATE)");
            // ExecuteNonQuery(conn, "INSERT INTO TAG (TAG_ID, TAG_NAME, COUNT, LAST_QUOTE) VALUES (361, 'Database', 0, SYSDATE)");

            // // 插入圈子
            // ExecuteNonQuery(conn, @"
            //     INSERT INTO CIRCLES (
            //         CIRCLE_ID, NAME, DESCRIPTION, OWNER_ID, CREATED_AT, CATEGORIES, AVATAR_URL, BANNER_URL
            //     ) VALUES (
            //         1, N'测试圈子', N'这是一个用于测试的圈子描述', 100222, SYSTIMESTAMP,
            //         N'科技,学习', 'https://example.com/avatar.png', 'https://example.com/banner.png'
            //     )");

            // 插入帖子
            // ExecuteNonQuery(conn, @"
            //     INSERT INTO POST (
            //         POST_ID, USER_ID, CONTENT, CREATED_AT, IS_DELETED, IS_HIDDEN,
            //         VIEWS, LIKES, DISLIKES, TITLE, CIRCLE_ID, SEARCH_TEXT
            //     ) VALUES (
            //         100162, 100295, '这是 Alice 发的关于 AI_test 的帖子', SYSDATE, 0, 0,
            //         10, 5, 0, 'AI_test 初学体验', 1, 'AI_test 初学'
            //     )");

            // ExecuteNonQuery(conn, @"
            //     INSERT INTO POST (
            //         POST_ID, USER_ID, CONTENT, CREATED_AT, IS_DELETED, IS_HIDDEN,
            //         VIEWS, LIKES, DISLIKES, TITLE, CIRCLE_ID, SEARCH_TEXT
            //     ) VALUES (
            //         100163, 100296, '这是 Bob 发的关于数据库的帖子', SYSDATE, 0, 0,
            //         15, 8, 0, '数据库优化技巧', 1, '数据库 优化'
            //     )");

            // 插入帖子标签
            // ExecuteNonQuery(conn, "INSERT INTO POSTTAG (POST_ID, TAG_ID) VALUES (100162, 360)");
            // ExecuteNonQuery(conn, "INSERT INTO POSTTAG (POST_ID, TAG_ID) VALUES (100163, 361)");

            // // 插入点赞
            // ExecuteNonQuery(conn, "INSERT INTO LIKES (USER_ID, TARGET_TYPE, TARGET_ID, LIKE_TYPE, LIKE_TIME) VALUES (100295, 'post', 100162, 1, SYSDATE)");
            // ExecuteNonQuery(conn, "INSERT INTO LIKES (USER_ID, TARGET_TYPE, TARGET_ID, LIKE_TYPE, LIKE_TIME) VALUES (100296, 'post', 100163, 1, SYSDATE)");
            

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // 删除测试数据
    public void DeleteTestData()
    {
        using var conn = GetConnection();
        conn.Open();

        using var tx = conn.BeginTransaction();
        try
        {
            // ExecuteNonQuery(conn, "DELETE FROM LIKES WHERE TARGET_ID IN (100151, 100152)"); //TARGET_ID
            // ExecuteNonQuery(conn, "DELETE FROM POSTTAG WHERE POST_ID IN (100151, 100152)");
            // ExecuteNonQuery(conn, "DELETE FROM POST WHERE POST_ID IN (100151, 100152)");
            // ExecuteNonQuery(conn, "DELETE FROM TAG WHERE TAG_ID IN (1, 2)");
            // ExecuteNonQuery(conn, "DELETE FROM CIRCLES WHERE CIRCLE_ID = 1");
            // ExecuteNonQuery(conn, "DELETE FROM USERS WHERE USER_ID IN (100295, 100296)");

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private void ExecuteNonQuery(OracleConnection conn, string sql)
    {
        using var cmd = new OracleCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }
}
