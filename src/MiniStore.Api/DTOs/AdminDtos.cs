namespace MiniStore.Api.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public CurrentUserDto User { get; set; } = new();
}

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string StoreSlug { get; set; } = string.Empty;
}

public class DashboardSummaryDto
{
    public int TodayAppointmentsCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public int TotalCustomersCount { get; set; }
    public decimal TodayEstimatedRevenue { get; set; }
    public List<AppointmentListItemDto> RecentAppointments { get; set; } = new();
}

public class AppointmentListItemDto
{
    public Guid Id { get; set; }
    public DateTime AppointmentAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    public string Status { get; set; } = string.Empty; // confirmed, completed, cancelled
}

public class CustomerListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public DateTime? LastVisitAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ServiceItemDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool Enabled { get; set; }
    public int SortOrder { get; set; }
}

public class UpsertServiceItemRequest
{
    public Guid? Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool Enabled { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class ServiceCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int ServicesCount { get; set; }
}

public class UpsertCategoryRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}

public class MemberCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalCount { get; set; }
    public int ValidDays { get; set; }
    public string? Description { get; set; }
    public int ActiveCardsCount { get; set; }
}

public class CreateMemberCardRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "times";
    public decimal Price { get; set; }
    public int TotalCount { get; set; } = 10;
    public int ValidDays { get; set; } = 365;
    public string? Description { get; set; }
}

public class CustomerCardDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public int RemainingCount { get; set; }
    public DateTime? ExpireAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BindCustomerCardRequest
{
    public Guid CustomerId { get; set; }
    public Guid CardId { get; set; }
}

public class DeductCardRequest
{
    public Guid CustomerCardId { get; set; }
    public int DeductCount { get; set; } = 1;
    public string? Remark { get; set; }
}

public class StoreSettingsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Hours { get; set; }
    public string? Announcement { get; set; }
}

public class UpdateStoreSettingsRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Hours { get; set; }
    public string? Announcement { get; set; }
}
