using AutoMapper;
using MediatR;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<int>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    private readonly ICacheService _cache;

    public CreateProductCommandHandler(IApplicationDbContext db, IMapper mapper, ICacheService cache)
    {
        _db = db;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var entity = _mapper.Map<Product>(request.Request);
        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);

        await _cache.RemoveByPrefixAsync("products:");

        return entity.Id;
    }
}