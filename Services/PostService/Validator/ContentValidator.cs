// refactoring begin
using PostService.DTOs;

namespace PostService.Validator;

public class ContentValidator : PublishValidator
{
    protected override string? DoValidate(PostPublishRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return "内容不能为空";
        return null;
    }
}
// refactoring end