namespace NetCaseStudy.Application.DTOs;

public sealed record CreateOrderRequest(IReadOnlyList<CreateOrderItem> Items);

public sealed record CreateOrderItem(int ProductId, int Quantity);