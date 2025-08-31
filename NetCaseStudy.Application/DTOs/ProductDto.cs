namespace NetCaseStudy.Application.DTOs;

public sealed record ProductDto(int Id, string Name, decimal Price, bool IsActive);

public sealed record CreateProductRequest(string Name, decimal Price);