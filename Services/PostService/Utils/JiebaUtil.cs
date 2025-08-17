using JiebaNet.Segmenter;

namespace PostService.Utils;

public class JiebaUtil
{
    public JiebaUtil()
    {
        // 确保字典文件存在，如果不存在，抛出更明确的异常
        var dictsPath = Path.Combine(AppContext.BaseDirectory, "Resources");
        if (!Directory.Exists(dictsPath) || !File.Exists(Path.Combine(dictsPath, "dict.txt")))
        {
            throw new InvalidOperationException($"Jieba.NET dictionary files not found at: {dictsPath}");
        }
    }
    
    private static readonly JiebaSegmenter Segmenter = new JiebaSegmenter();
    
    public static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Enumerable.Empty<string>();
        }

        // 1. 将输入文本转为小写，处理大小写不一致的问题
        var lowerText = text.ToLowerInvariant();
        
        // 2. 使用 JiebaSegmenter 进行分词
        var words = Segmenter.Cut(lowerText, cutAll: false);
        
        // 3. 过滤掉空字符串和停用词
        return words.Where(w => !string.IsNullOrEmpty(w) && !StopWords.Value.Contains(w));
    }
    
    private static readonly Lazy<HashSet<string>> StopWords = new Lazy<HashSet<string>>(() => LoadStopWords());

    private static HashSet<string> LoadStopWords()
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "stopwords.txt");
        
        if (!File.Exists(filePath))
        {
            // 如果文件不存在，可以记录日志或返回空集合
            Console.WriteLine($"Stop words file not found at: {filePath}");
            return stopWords;
        }

        foreach (var line in File.ReadLines(filePath))
        {
            // 移除首尾空白，并转换为小写
            var word = line.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(word))
            {
                stopWords.Add(word);
            }
        }
        return stopWords;
    }
}