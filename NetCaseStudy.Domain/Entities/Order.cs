using NetCaseStudy.Domain.BaseModels;

namespace NetCaseStudy.Domain.Entities;

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Cancelled
}

public class Order : AuditableEntityInt
{
    public string UserId { get; set; } = string.Empty;

    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);

    public void MarkPaid() => Status = OrderStatus.Paid;

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Order must be paid before shipping.");
        Status = OrderStatus.Shipped;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Shipped order cannot be cancelled.");
        Status = OrderStatus.Cancelled;
    }
}