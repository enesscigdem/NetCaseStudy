using Microsoft.EntityFrameworkCore;
using NetCaseStudy.Domain.Entities;

namespace NetCaseStudy.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}