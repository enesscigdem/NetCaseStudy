using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Features.Orders.Queries;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _db;
    public GetOrderByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken);
        if (order is null)
        {
            return null;
        }
        var items = order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            _db.Products.AsNoTracking().First(p => p.Id == i.ProductId).Name,
            i.Quantity,
            i.UnitPrice
        )).ToList();
        return new OrderDto(order.Id, order.UserId, order.Status.ToString(), items, order.Total);
    }
}