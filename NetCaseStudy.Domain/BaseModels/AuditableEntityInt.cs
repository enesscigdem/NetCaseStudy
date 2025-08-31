namespace NetCaseStudy.Domain.BaseModels;

public abstract class AuditableEntityInt : EntityBaseInt, IAuditEntity
{
    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }
}