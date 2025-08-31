using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Features.Products.Queries;

/// <summary>
/// Query for retrieving products with paging, filtering and sorting.
/// </summary>
public record ListProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null,
    bool Descending = false
) : IRequest<PagedResult<ProductDto>>;

/// <summary>
/// Handler for <see cref="ListProductsQuery"/>.
/// </summary>
public class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    public ListProductsQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }
        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => request.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            _ => request.Descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}