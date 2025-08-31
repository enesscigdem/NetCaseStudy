using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Features.Orders.Commands;

public record CancelOrderCommand(int Id, string UserId, bool IsAdmin) : IRequest<bool>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public CancelOrderCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken);
        if (order is null)
        {
            return false;
        }
        if (!request.IsAdmin && !string.Equals(order.UserId, request.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel this order.");
        }
        try
        {
            order.Cancel();
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}