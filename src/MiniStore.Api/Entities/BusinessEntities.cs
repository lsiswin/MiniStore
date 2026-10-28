using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniStore.Api.Entities;

public class ServiceCategory : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public List<ServiceItem> Items { get; set; } = new();
}

public class ServiceItem : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid CategoryId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public int DurationMinutes { get; set; } = 60;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool Enabled { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}

public class Customer : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public int VisitCount { get; set; } = 0;

    public DateTime? LastVisitAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class Appointment : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid StoreId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime AppointmentAt { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // pending, confirmed, completed, cancelled

    [MaxLength(50)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class MemberCard : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Type { get; set; } = "times"; // times, stored_value

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public int TotalCount { get; set; } = 10;

    public int ValidDays { get; set; } = 365;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class CustomerCard : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid CardId { get; set; }

    [MaxLength(100)]
    public string CardName { get; set; } = string.Empty;

    public int RemainingCount { get; set; }

    public DateTime? ExpireAt { get; set; }
}

public class CardRecord : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid CustomerCardId { get; set; }

    public int Change { get; set; } = -1; // 例如核销扣减 1

    public int RemainingAfter { get; set; }

    [MaxLength(200)]
    public string? Remark { get; set; }
}

public class IndustryTemplate : BaseEntity
{
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty; // BeautyNail, HairSalon, etc.

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string SeedJson { get; set; } = string.Empty;
}
