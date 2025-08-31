namespace NetCaseStudy.Application.DTOs;

public class CursorPagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public string? NextCursor { get; init; }
    public int PageSize { get; init; }
}