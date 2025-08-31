using System.ComponentModel.DataAnnotations;

namespace NetCaseStudy.Domain.BaseModels;

public abstract class EntityBaseInt : IIntEntity, IIsDeletedEntity, IActivateableEntity
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsActive { get; set; } = true;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}