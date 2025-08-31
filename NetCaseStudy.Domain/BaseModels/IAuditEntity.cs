namespace NetCaseStudy.Domain.BaseModels;

public interface IAuditEntity : IIsDeletedEntity, IActivateableEntity
{
    DateTime CreatedAt { get; set; }

    string? CreatedBy { get; set; }

    DateTime? ModifiedAt { get; set; }

    string? ModifiedBy { get; set; }
}