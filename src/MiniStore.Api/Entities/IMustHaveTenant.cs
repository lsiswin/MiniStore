namespace MiniStore.Api.Entities;

public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
