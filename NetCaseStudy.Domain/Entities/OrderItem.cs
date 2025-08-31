using NetCaseStudy.Domain.BaseModels;

namespace NetCaseStudy.Domain.Entities;

public class OrderItem : AuditableEntityInt
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}