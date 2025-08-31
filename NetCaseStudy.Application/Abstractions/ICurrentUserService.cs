namespace NetCaseStudy.Application.Abstractions;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }
}