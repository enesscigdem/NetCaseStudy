namespace NetCaseStudy.Application.DTOs;

public sealed record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

public sealed record OrderDto(int Id, string UserId, string Status, List<OrderItemDto> Items, decimal Total);