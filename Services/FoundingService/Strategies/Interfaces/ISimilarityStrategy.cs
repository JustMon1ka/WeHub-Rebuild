/*
重构代码：
refactoring with Strategies Pattern
*/
namespace MyBackend.Strategies.Interfaces;

public interface ISimilarityStrategy
{
    /// <summary>
    /// 计算两个用户画像之间的相似度分数
    /// </summary>
    /// <param name="userProfile">当前用户的标签权重 (Tag -> Weight)</param>
    /// <param name="targetProfile">目标用户的标签权重 (Tag -> Weight)</param>
    /// <returns>相似度分数 (通常 0.0 到 1.0)</returns>
    double Calculate(Dictionary<string, int> userProfile, Dictionary<string, int> targetProfile);
}