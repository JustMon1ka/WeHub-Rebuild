using PostService.DTOs;

namespace PostService.Validator;

public abstract class PublishValidator
{
    protected PublishValidator? _next;

    public PublishValidator SetNext(PublishValidator next)
    {
        _next = next;
        return next;
    }

    public string? Validate(PostPublishRequest request)
    {
        string? error = DoValidate(request);
        if (error != null) return error;

        return _next?.Validate(request);
    }

    protected abstract string? DoValidate(PostPublishRequest request);
}