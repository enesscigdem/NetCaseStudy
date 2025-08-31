namespace NetCaseStudy.Domain.Entities;

using NetCaseStudy.Domain.BaseModels;

public class Product : AuditableEntityInt
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}