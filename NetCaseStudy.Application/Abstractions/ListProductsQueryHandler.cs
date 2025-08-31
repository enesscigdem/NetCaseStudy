using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Application.Features.Products.Queries;

namespace NetCaseStudy.Application.Abstractions;

public class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public ListProductsQueryHandler(IApplicationDbContext db, IMapper mapper, ICacheService cache)
    {
        _db = db;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PagedResult<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey =
            $"products:{request.Page}:{request.PageSize}:{request.Search}:{request.MinPrice}:{request.MaxPrice}:{request.SortBy}:{request.Descending}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var query = _db.Products.AsNoTracking().Where(p => p.IsActive && !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term));
            }

            if (request.MinPrice.HasValue) query = query.Where(p => p.Price >= request.MinPrice.Value);
            if (request.MaxPrice.HasValue) query = query.Where(p => p.Price <= request.MaxPrice.Value);

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => request.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                _ => request.Descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ProductDto>
            {
                Items = _mapper.Map<IReadOnlyCollection<ProductDto>>(items),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }, TimeSpan.FromMinutes(5));
    }
}