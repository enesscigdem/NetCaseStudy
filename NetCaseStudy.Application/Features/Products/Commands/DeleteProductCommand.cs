using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;

namespace NetCaseStudy.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _db;

    private readonly ICacheService _cache;

    public DeleteProductCommandHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);
        if (product is null) return false;

        product.IsDeleted = true;
        product.IsActive = false;

        await _db.SaveChangesAsync(ct);

        await _cache.RemoveByPrefixAsync("products:");

        return true;
    }
}