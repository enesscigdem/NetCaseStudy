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
    public CreateProductCommandHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Product>(request.Request);
        entity.IsActive = true;
        _db.Products.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}