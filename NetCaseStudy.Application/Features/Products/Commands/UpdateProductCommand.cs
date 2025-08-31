using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;

namespace NetCaseStudy.Application.Features.Products.Commands;

public record UpdateProductCommand(int Id, CreateProductRequest Request) : IRequest<bool>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    private readonly ICacheService _cache;

    public UpdateProductCommandHandler(IApplicationDbContext db, IMapper mapper, ICacheService cache)
    {
        _db = db;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);
        if (product is null) return false;

        product.Name = request.Request.Name;
        product.Price = request.Request.Price;
        product.IsActive = true;

        await _db.SaveChangesAsync(ct);

        await _cache.RemoveByPrefixAsync("products:");

        return true;
    }
}