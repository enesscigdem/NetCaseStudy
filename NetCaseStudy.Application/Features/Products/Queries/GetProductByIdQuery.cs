using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Application.Abstractions;
using NetCaseStudy.Application.DTOs;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Features.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    public GetProductByIdQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);
        return entity is null ? null : _mapper.Map<ProductDto>(entity);
    }
}