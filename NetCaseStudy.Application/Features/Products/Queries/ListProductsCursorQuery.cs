using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;

namespace NetCaseStudy.Application.Features.Products.Queries;

public record ListProductsCursorQuery(
    int PageSize = 20,
    string? Cursor = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = "id",
    bool Descending = false
) : IRequest<CursorPagedResult<ProductDto>>;

public class ListProductsCursorQueryHandler
    : IRequestHandler<ListProductsCursorQuery, CursorPagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public ListProductsCursorQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CursorPagedResult<ProductDto>> Handle(
        ListProductsCursorQuery request, CancellationToken ct)
    {
        var q = _db.Products.AsNoTracking().Where(p => p.IsActive && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(term));
        }
        if (request.MinPrice.HasValue) q = q.Where(p => p.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) q = q.Where(p => p.Price <= request.MaxPrice.Value);

        int? lastId = null;
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            if (int.TryParse(System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(request.Cursor)), out var id))
            {
                lastId = id;
            }
        }

        IOrderedQueryable<Domain.Entities.Product> ordered = request.SortBy?.ToLower() switch
        {
            "name"  => request.Descending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
            "price" => request.Descending ? q.OrderByDescending(p => p.Price): q.OrderBy(p => p.Price),
            _       => request.Descending ? q.OrderByDescending(p => p.Id)   : q.OrderBy(p => p.Id)
        };

        if (lastId.HasValue && (request.SortBy ?? "id").ToLower() == "id")
        {
            if (request.Descending)
                ordered = (IOrderedQueryable<Domain.Entities.Product>)ordered.Where(p => p.Id < lastId.Value);
            else
                ordered = (IOrderedQueryable<Domain.Entities.Product>)ordered.Where(p => p.Id > lastId.Value);
        }

        var items = await ordered.Take(request.PageSize).ToListAsync(ct);
        var nextCursor = (items.Count == request.PageSize)
            ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(items[^1].Id.ToString()))
            : null;

        return new CursorPagedResult<ProductDto>
        {
            Items = _mapper.Map<IReadOnlyCollection<ProductDto>>(items),
            PageSize = request.PageSize,
            NextCursor = nextCursor
        };
    }
}
