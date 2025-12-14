/*
重构代码：
*/
using System.Data;
using MyBackend.Facades.Interfaces;
using MyBackend.Infrastructure;
using MyBackend.Models;
using Oracle.ManagedDataAccess.Client;

namespace MyBackend.Facades;

public class RecommendDbFacade : IRecommendDbFacade
{
    private readonly IDbConnectionFactory _dbFactory;

    public RecommendDbFacade(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    private IDbConnection GetConnection()
    {
        var conn = _dbFactory.CreateConnection();
        conn.Open();
        return conn;
    }

    public async Task<IEnumerable<RecommendResult>> FetchPersonalizedTopicsAsync(int userId, int topK)
    {
        var results = new List<RecommendResult>();
        using var conn = GetConnection();

        string sql = @"
            SELECT * FROM (
                SELECT t.TAG_NAME, 
                       (SELECT COUNT(*) FROM POST p2 JOIN POSTTAG pt2 ON p2.POST_ID = pt2.POST_ID 
                        WHERE pt2.TAG_ID = t.TAG_ID AND NVL(p2.IS_DELETED,0)=0 AND NVL(p2.IS_HIDDEN,0)=0) AS REAL_CNT
                FROM LIKES l
                JOIN POST p ON l.TARGET_ID = p.POST_ID AND l.TARGET_TYPE = 'post'
                JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
                JOIN TAG t ON pt.TAG_ID = t.TAG_ID
                WHERE l.USER_ID = :p_user_id AND l.LIKE_TIME >= SYSDATE - 7
                GROUP BY t.TAG_ID, t.TAG_NAME
                ORDER BY COUNT(*) DESC
            ) WHERE ROWNUM <= :p_top_k";

        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new OracleParameter("p_user_id", userId));
        cmd.Parameters.Add(new OracleParameter("p_top_k", topK));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new RecommendResult
            {
                Topic = "#" + reader["TAG_NAME"],
                Count = Convert.ToInt32(reader["REAL_CNT"]),
                Category = "热门话题",
                SubCategory = "推荐",
                Desc = "最近一周的个性化话题推荐"
            });
        }
        return results;
    }

    public async Task<IEnumerable<RecommendResult>> FetchGlobalHotTopicsAsync(int topK)
    {
        var results = new List<RecommendResult>();
        using var conn = GetConnection();

        string sql = @"
            SELECT * FROM (
                SELECT t.TAG_NAME,
                    (SELECT COUNT(*) FROM POST p2 JOIN POSTTAG pt2 ON p2.POST_ID = pt2.POST_ID 
                     WHERE pt2.TAG_ID = t.TAG_ID AND NVL(p2.IS_DELETED,0)=0 AND NVL(p2.IS_HIDDEN,0)=0) AS REAL_CNT
                FROM LIKES l
                JOIN POST p ON l.TARGET_ID = p.POST_ID AND l.TARGET_TYPE = 'post'
                JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
                JOIN TAG t ON pt.TAG_ID = t.TAG_ID
                WHERE TRUNC(l.LIKE_TIME) = TRUNC(SYSDATE)
                GROUP BY t.TAG_ID, t.TAG_NAME
                ORDER BY COUNT(*) DESC
            ) WHERE ROWNUM <= :p_top_k";

        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new OracleParameter("p_top_k", topK));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new RecommendResult
            {
                Topic = "#" + reader["TAG_NAME"],
                Count = Convert.ToInt32(reader["REAL_CNT"]),
                Category = "今日热点",
                SubCategory = "推荐",
                Desc = "今天全站最热门的话题"
            });
        }
        return results;
    }

    public async Task<Dictionary<int, Dictionary<string, int>>> FetchAllUserTagProfilesAsync()
    {
        var profiles = new Dictionary<int, Dictionary<string, int>>();
        using var conn = GetConnection();
        
        string sql = @"SELECT ut.USER_ID, t.TAG_NAME FROM USERTAG ut JOIN TAG t ON ut.TAG_ID = t.TAG_ID";
        
        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int uid = reader.GetInt32(0);
            string tag = reader.GetString(1);

            if (!profiles.ContainsKey(uid)) profiles[uid] = new Dictionary<string, int>();
            profiles[uid][tag] = 1;
        }
        return profiles;
    }

    public async Task<HashSet<int>> FetchFollowedUserIdsAsync(int userId)
    {
        var followed = new HashSet<int>();
        using var conn = GetConnection();
        
        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = "SELECT FOLLOWEE_ID FROM FOLLOW WHERE FOLLOWER_ID = :p_uid";
        cmd.Parameters.Add(new OracleParameter("p_uid", userId));
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            followed.Add(reader.GetInt32(0));
        }
        return followed;
    }

    public async Task<IEnumerable<UserProfileResult>> FetchUserDetailsAsync(IEnumerable<int> userIds)
    {
        var users = new List<UserProfileResult>();
        if (!userIds.Any()) return users;

        using var conn = GetConnection();
        
        // 注意：Oracle IN 查询建议处理，这里简化展示
        var idList = string.Join(",", userIds); 
        string sql = $@"
            SELECT u.USER_ID, u.USERNAME, 
                   NVL(p.NICKNAME, u.USERNAME) AS NICKNAME, 
                   NVL(p.AVATAR_URL, p.PROFILE_URL) AS AVATAR_URL
            FROM USERS u
            LEFT JOIN USERPROFILE p ON u.USER_ID = p.USER_ID
            WHERE u.USER_ID IN ({idList})";

        using var cmd = (OracleCommand)conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new UserProfileResult
            {
                user_id = reader.GetInt32(0),
                username = reader["USERNAME"]?.ToString() ?? "未知用户",
                nickname = reader["NICKNAME"]?.ToString() ?? "未知用户",
                avatar_url = reader["AVATAR_URL"]?.ToString()
            });
        }
        return users;
    }
}