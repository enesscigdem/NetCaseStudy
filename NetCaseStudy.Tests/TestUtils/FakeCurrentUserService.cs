using NetCaseStudy.Application.Abstractions;

namespace NetCaseStudy.Tests.TestUtils;

public class FakeCurrentUserService : ICurrentUserService
{
    public string? UserId { get; }
    public FakeCurrentUserService(string? userId = "test-user") => UserId = userId;
}