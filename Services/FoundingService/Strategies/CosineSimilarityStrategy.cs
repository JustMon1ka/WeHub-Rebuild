/*
重构代码：
*/
using MyBackend.Strategies.Interfaces;

namespace MyBackend.Strategies;

public class CosineSimilarityStrategy : ISimilarityStrategy
{
    public double Calculate(Dictionary<string, int> profileA, Dictionary<string, int> profileB)
    {
        // 如果任意一方画像为空，相似度为0
        if (profileA == null || profileB == null || profileA.Count == 0 || profileB.Count == 0)
        {
            return 0.0;
        }

        // 1. 获取所有出现的标签集合（并集）
        var allKeys = profileA.Keys.Union(profileB.Keys);

        double dotProduct = 0; // 点积
        double normA = 0;      // A的模长平方
        double normB = 0;      // B的模长平方

        // 2. 遍历向量维度进行计算
        foreach (var key in allKeys)
        {
            // 获取向量分量，如果该用户没有此标签，则为0
            int valA = profileA.ContainsKey(key) ? profileA[key] : 0;
            int valB = profileB.ContainsKey(key) ? profileB[key] : 0;

            dotProduct += valA * valB;
            normA += valA * valA;
            normB += valB * valB;
        }

        // 3. 防止除以零
        if (normA == 0 || normB == 0)
        {
            return 0.0;
        }

        // 4. 计算余弦值: (A . B) / (|A| * |B|)
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}