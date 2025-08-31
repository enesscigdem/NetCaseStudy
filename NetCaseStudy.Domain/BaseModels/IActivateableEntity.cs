namespace NetCaseStudy.Domain.BaseModels;

public interface IActivateableEntity
{
    bool IsActive { get; set; }
}