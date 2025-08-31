using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Features.Orders.Commands;

public record CreateOrderCommand(CreateOrderRequest Request, string UserId) : IRequest<int>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateOrderCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Request.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive && !p.IsDeleted)
            .ToListAsync(cancellationToken);
        if (products.Count != request.Request.Items.Count)
        {
            throw new InvalidOperationException("One or more products are invalid or inactive.");
        }
        var order = new Order { UserId = request.UserId };
        foreach (var item in request.Request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}