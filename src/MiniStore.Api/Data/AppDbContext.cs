using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Entities;
using MiniStore.Api.Services;

namespace MiniStore.Api.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<MerchantUser> Users => Set<MerchantUser>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MemberCard> MemberCards => Set<MemberCard>();
    public DbSet<CustomerCard> CustomerCards => Set<CustomerCard>();
    public DbSet<CardRecord> CardRecords => Set<CardRecord>();
    public DbSet<IndustryTemplate> IndustryTemplates => Set<IndustryTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store Slug unique index
        modelBuilder.Entity<Store>()
            .HasIndex(s => s.Slug)
            .IsUnique();

        // Customer unique index per tenant phone
        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.TenantId, c.Phone });

        // User unique username
        modelBuilder.Entity<MerchantUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Setup Global Query Filters for all IMustHaveTenant entities
        // If CurrentTenantId is not set, we don't filter (e.g. for migrations or public slug lookup),
        // but when CurrentTenantId is set in Admin context, filter automatically!
        modelBuilder.Entity<MerchantUser>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<Store>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<ServiceCategory>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<ServiceItem>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<MemberCard>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<CustomerCard>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);

        modelBuilder.Entity<CardRecord>()
            .HasQueryFilter(e => !_tenantContext.CurrentTenantId.HasValue || e.TenantId == _tenantContext.CurrentTenantId.Value);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.Entity is IMustHaveTenant tenantEntity)
            {
                if (entry.State == EntityState.Added && tenantEntity.TenantId == Guid.Empty)
                {
                    if (_tenantContext.CurrentTenantId.HasValue)
                    {
                        tenantEntity.TenantId = _tenantContext.CurrentTenantId.Value;
                    }
                }
            }

            if (entry.Entity is BaseEntity baseEntity && entry.State == EntityState.Added)
            {
                if (baseEntity.CreatedAt == default)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
