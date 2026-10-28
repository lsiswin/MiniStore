namespace MiniStore.Api.DTOs;

public class StorePublicDto
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Hours { get; set; }
    public string? Announcement { get; set; }
    public List<CategoryPublicDto> Categories { get; set; } = new();
}

public class CategoryPublicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<ServicePublicDto> Services { get; set; } = new();
}

public class ServicePublicDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}

public class BookAppointmentRequest
{
    public string Slug { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public DateTime AppointmentAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AppointmentResultDto
{
    public Guid AppointmentId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AppointmentAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string Message { get; set; } = "预约已提交，请留意门店确认";
}

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public DateTime? LastVisitAt { get; set; }
    public string? Notes { get; set; }
    public List<CustomerCardItemDto> Cards { get; set; } = new();
}

public class CustomerCardItemDto
{
    public Guid Id { get; set; }
    public string CardName { get; set; } = string.Empty;
    public int RemainingCount { get; set; }
    public DateTime? ExpireAt { get; set; }
}

public class CustomerAppointmentDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AppointmentAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateCustomerProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
