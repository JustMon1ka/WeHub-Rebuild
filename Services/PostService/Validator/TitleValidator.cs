using PostService.DTOs;

namespace PostService.Validator;

public class TitleValidator : PublishValidator
{
    protected override string? DoValidate(PostPublishRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return "标题不能为空";
        return null;
    }
}