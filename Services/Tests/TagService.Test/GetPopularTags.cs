using NUnit.Framework.Interfaces;
using Xunit.Abstractions;

namespace TagService.Test;

public class GetPopularTags
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _auth;
    private readonly HttpClient _tag;
}