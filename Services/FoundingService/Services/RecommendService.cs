using MyBackend.Models;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;

namespace MyBackend.Services;

public class RecommendService
{
    private readonly IConfiguration _config;
    public RecommendService(IConfiguration config) => _config = config;

    private OracleConnection GetConnection()
        => new OracleConnection(_config.GetConnectionString("DefaultConnection"));

    // 🔹 本周热门话题（个性化推荐）
    public IEnumerable<RecommendResult> RecommendTopics(int userId, int topK = 4)
    {
        var results = new List<RecommendResult>();
        using var conn = GetConnection();
        conn.Open();

        string sql = $@"
            SELECT *
            FROM (
                SELECT t.TAG_ID,
                    t.TAG_NAME,
                    (SELECT COUNT(*)
                        FROM POST p2
                        JOIN POSTTAG pt2 ON p2.POST_ID = pt2.POST_ID
                        WHERE pt2.TAG_ID = t.TAG_ID
                            AND NVL(p2.IS_DELETED,0)=0
                            AND NVL(p2.IS_HIDDEN,0)=0
                    ) AS REAL_CNT
                FROM LIKES l
                JOIN POST p ON l.TARGET_ID = p.POST_ID AND l.TARGET_TYPE = 'post'
                JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
                JOIN TAG t ON pt.TAG_ID = t.TAG_ID
                WHERE l.USER_ID = :p_user_id
                AND l.LIKE_TIME >= SYSDATE - 7
                GROUP BY t.TAG_ID, t.TAG_NAME
                ORDER BY COUNT(*) DESC
            )
            WHERE ROWNUM <= {topK}";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("p_user_id", userId));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new RecommendResult
            {
                Topic = "#" + reader.GetString(reader.GetOrdinal("TAG_NAME")),
                Count = reader.GetInt32(reader.GetOrdinal("REAL_CNT")), // ✅ 只取真实帖子数
                Category = "热门话题",
                SubCategory = "推荐",
                Desc = "最近一周的个性化话题推荐"
            });
        }
        return results;
    }



    // 🔹 今日热点（全局热门）
    public IEnumerable<RecommendResult> RecommendHot(int topK = 3)
    {
        var results = new List<RecommendResult>();
        using var conn = GetConnection();
        conn.Open();

        string sql = $@"
            SELECT *
            FROM (
                SELECT t.TAG_ID,
                    t.TAG_NAME,
                    -- ✅ 查该标签下真实帖子数
                    (SELECT COUNT(*)
                        FROM POST p2
                        JOIN POSTTAG pt2 ON p2.POST_ID = pt2.POST_ID
                        WHERE pt2.TAG_ID = t.TAG_ID
                            AND NVL(p2.IS_DELETED,0)=0
                            AND NVL(p2.IS_HIDDEN,0)=0
                    ) AS REAL_CNT
                FROM LIKES l
                JOIN POST p ON l.TARGET_ID = p.POST_ID AND l.TARGET_TYPE = 'post'
                JOIN POSTTAG pt ON p.POST_ID = pt.POST_ID
                JOIN TAG t ON pt.TAG_ID = t.TAG_ID
                WHERE TRUNC(l.LIKE_TIME) = TRUNC(SYSDATE)
                GROUP BY t.TAG_ID, t.TAG_NAME
                ORDER BY COUNT(*) DESC   -- 排序仍然用今日点赞数
            )
            WHERE ROWNUM <= {topK}";

        using var cmd = new OracleCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            results.Add(new RecommendResult
            {
                Topic = "#" + reader.GetString(reader.GetOrdinal("TAG_NAME")),
                Count = reader.GetInt32(reader.GetOrdinal("REAL_CNT")), // ✅ 改成真实帖子数
                Category = "今日热点",
                SubCategory = "推荐",
                Desc = "今天全站最热门的话题"
            });
        }
        return results;
    }


    // 🔹 推荐关注的用户（基于标签兴趣相似度）
    // 前提：不能推荐已经关注了的用户
    public IEnumerable<object> RecommendUsers(int userId, int topN = 2)
    {
        var results = new List<object>();
        using var conn = GetConnection();
        conn.Open();

        // 1. 查用户兴趣标签
        string sql = @"
            SELECT ut.USER_ID, t.TAG_NAME
            FROM USERTAG ut
            JOIN TAG t ON ut.TAG_ID = t.TAG_ID";

        var profiles = new Dictionary<int, Dictionary<string, int>>();
        using (var cmd = new OracleCommand(sql, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int uid = reader.GetInt32(0);
                string tag = reader.GetString(1);

                if (!profiles.ContainsKey(uid))
                    profiles[uid] = new Dictionary<string, int>();

                profiles[uid][tag] = 1;
            }
        }

        if (!profiles.ContainsKey(userId)) return results;

        // 2. 查出当前用户已经关注的人
        var followed = new HashSet<int>();
        string followSql = "SELECT FOLLOWEE_ID FROM FOLLOW WHERE FOLLOWER_ID = :p_uid";
        using (var followCmd = new OracleCommand(followSql, conn))
        {
            followCmd.Parameters.Add(new OracleParameter("p_uid", userId));
            using var fReader = followCmd.ExecuteReader();
            while (fReader.Read())
            {
                followed.Add(fReader.GetInt32(0));
            }
        }

        // 3. 计算相似度
        var targetProfile = profiles[userId];
        var scores = new List<(int, double)>();

        foreach (var kv in profiles)
        {
            int uid = kv.Key;
            if (uid == userId) continue;       // 不能推荐自己
            if (followed.Contains(uid)) continue; // 已关注过的用户不推荐

            double sim = CosineSim(targetProfile, kv.Value);
            scores.Add((uid, sim));
        }

        // 4. 查用户名并组装结果
        string userSql = "SELECT USERNAME FROM USERS WHERE USER_ID = :p_uid";
        foreach (var (uid, score) in scores.OrderByDescending(s => s.Item2).Take(topN))
        {
            using var userCmd = new OracleCommand(userSql, conn);
            userCmd.Parameters.Add(new OracleParameter("p_uid", uid));

            var usernameObj = userCmd.ExecuteScalar();
            string username = usernameObj != null ? usernameObj.ToString()! : "未知用户";

            results.Add(new
            {
                user_id = uid,
                username = username,
                similarity = score
            });
        }

        return results;
    }
    // 🔹 工具函数：余弦相似度
    private double CosineSim(Dictionary<string, int> a, Dictionary<string, int> b)
    {
        var allKeys = a.Keys.Union(b.Keys);
        double dot = 0, normA = 0, normB = 0;
        foreach (var k in allKeys)
        {
            int va = a.ContainsKey(k) ? a[k] : 0;
            int vb = b.ContainsKey(k) ? b[k] : 0;
            dot += va * vb;
            normA += va * va;
            normB += vb * vb;
        }
        return (normA == 0 || normB == 0) ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
