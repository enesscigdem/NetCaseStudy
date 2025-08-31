using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;

namespace NetCaseStudy.Application.Features.Orders.Queries;

public record ListOrdersQuery(
    int Page = 1,
    int PageSize = 10,
    string? UserId = null,
    bool IsAdmin = false
) : IRequest<PagedResult<OrderDto>>;

public class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IApplicationDbContext _db;
    public ListOrdersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<OrderDto>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted);

        if (!request.IsAdmin && !string.IsNullOrEmpty(request.UserId))
        {
            query = query.Where(o => o.UserId == request.UserId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var resultItems = new List<OrderDto>();
        foreach (var order in orders)
        {
            var items = new List<OrderItemDto>();
            foreach (var item in order.Items)
            {
                var product = await _db.Products.AsNoTracking().FirstAsync(p => p.Id == item.ProductId, cancellationToken);
                items.Add(new OrderItemDto(item.ProductId, product.Name, item.Quantity, item.UnitPrice));
            }
            resultItems.Add(new OrderDto(order.Id, order.UserId, order.Status.ToString(), items, order.Total));
        }

        return new PagedResult<OrderDto>
        {
            Items = resultItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}