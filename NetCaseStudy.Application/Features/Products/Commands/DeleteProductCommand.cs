using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;

namespace NetCaseStudy.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteProductCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);
        if (entity is null)
            return false;
        entity.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}