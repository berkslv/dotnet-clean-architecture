using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }
    
    private readonly List<object> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<object> DomainEvents
    {
        get
        {
            return _domainEvents.AsReadOnly();
        }
    }

    public void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(object domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}