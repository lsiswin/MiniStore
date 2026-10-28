using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniStore.Api.Entities;

public class Tenant : BaseEntity
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Plan { get; set; } = "standard";

    [MaxLength(20)]
    public string Status { get; set; } = "active";

    public DateTime? ExpireAt { get; set; }
}

public class MerchantUser : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = "owner";
}

public class Store : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Logo { get; set; }

    [MaxLength(500)]
    public string? Banner { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Hours { get; set; } = "10:00 - 21:00";

    [MaxLength(500)]
    public string? Announcement { get; set; }
}
