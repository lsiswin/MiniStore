using Microsoft.EntityFrameworkCore;
using MiniStore.Api.Entities;

namespace MiniStore.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Ensure SQLite / DB schema is created
        await context.Database.EnsureCreatedAsync();

        if (await context.Tenants.AnyAsync())
        {
            return; // DB already seeded
        }

        // 1. Create Demo Tenant
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "缪斯美甲美睫工作室",
            Plan = "pro",
            Status = "active",
            ExpireAt = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);

        // 2. Create Demo Merchant User (admin / 123456)
        var user = new MerchantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            DisplayName = "缪斯店长",
            Phone = "13800008888",
            Role = "owner",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);

        // 3. Create Store
        var storeId = Guid.NewGuid();
        var store = new Store
        {
            Id = storeId,
            TenantId = tenantId,
            Name = "MUSE NAIL · 缪斯日式美睫美甲 (万达旗舰店)",
            Slug = "muse-nail",
            Logo = "https://images.unsplash.com/photo-1604654894610-df63bc536371?w=200&auto=format&fit=crop&q=80",
            Banner = "https://images.unsplash.com/photo-1519014816548-bf5fe059798b?w=1200&auto=format&fit=crop&q=80",
            Phone = "138-0000-8888",
            Address = "万达广场金街 2 楼 208 室（星巴克楼上）",
            Hours = "10:00 - 21:30",
            Announcement = "✨ 新客到店享 8.8 折优惠，支持次卡核销，提前预约无需排队！",
            CreatedAt = DateTime.UtcNow
        };
        context.Stores.Add(store);

        // 4. Create Categories
        var catNail = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "精选美甲",
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow
        };
        var catLash = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "轻奢美睫",
            SortOrder = 2,
            CreatedAt = DateTime.UtcNow
        };
        var catSkin = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "面部护理",
            SortOrder = 3,
            CreatedAt = DateTime.UtcNow
        };
        context.ServiceCategories.AddRange(catNail, catLash, catSkin);

        // 5. Create Services
        var services = new List<ServiceItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catNail.Id,
                Name = "纯色跳色手部美甲 (含精致建构+修死皮)",
                Price = 98m,
                DurationMinutes = 60,
                ImageUrl = "https://images.unsplash.com/photo-1632345031435-8727f6897d53?w=500&auto=format&fit=crop&q=80",
                Description = "包含日式精修死皮、手部甲型修整、高保真色胶2色跳色。",
                Enabled = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catNail.Id,
                Name = "日式小红书爆款晕染渐变款",
                Price = 188m,
                DurationMinutes = 90,
                ImageUrl = "https://images.unsplash.com/photo-1599940824399-b87987ceb72a?w=500&auto=format&fit=crop&q=80",
                Description = "ins风格热门冰透晕染、贝壳片、金属饰品点缀，显白高级。",
                Enabled = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catNail.Id,
                Name = "法式轻奢镶钻高级定制款",
                Price = 268m,
                DurationMinutes = 100,
                ImageUrl = "https://images.unsplash.com/photo-1519014816548-bf5fe059798b?w=500&auto=format&fit=crop&q=80",
                Description = "施华洛世奇进口水钻镶嵌，极致奢华优雅，适合婚嫁与聚会。",
                Enabled = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catNail.Id,
                Name = "手部深层滋润护理 SPA",
                Price = 68m,
                DurationMinutes = 30,
                ImageUrl = "https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=500&auto=format&fit=crop&q=80",
                Description = "手部去角质磨砂、热敷手膜、滋润乳霜按摩，淡化手部细纹。",
                Enabled = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catLash.Id,
                Name = "日式单根自然清新美睫 (100根)",
                Price = 158m,
                DurationMinutes = 60,
                ImageUrl = "https://images.unsplash.com/photo-1583001931096-959e9a1a6223?w=500&auto=format&fit=crop&q=80",
                Description = "婴儿弯柔软睫毛，无感嫁接，宛若天生无负担。",
                Enabled = true,
                SortOrder = 5,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catLash.Id,
                Name = "女团漫画眼太阳花浓密款 (180根)",
                Price = 238m,
                DurationMinutes = 80,
                ImageUrl = "https://images.unsplash.com/photo-1560066984-138dadb4c035?w=500&auto=format&fit=crop&q=80",
                Description = "长短错落太阳花排列，放大双眸，自带眼线效果。",
                Enabled = true,
                SortOrder = 6,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = catSkin.Id,
                Name = "小气泡深层毛孔清洁护理",
                Price = 128m,
                DurationMinutes = 45,
                ImageUrl = "https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=500&auto=format&fit=crop&q=80",
                Description = "真空负压吸附黑头白头，水氧补水收缩毛孔。",
                Enabled = true,
                SortOrder = 7,
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Services.AddRange(services);

        // 6. Create Demo Customers
        var cust1 = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "林小雨",
            Phone = "13912345678",
            VisitCount = 3,
            LastVisitAt = DateTime.UtcNow.AddDays(-2),
            Notes = "喜欢浅粉色系，指甲偏软易断",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        var cust2 = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "王诗雅",
            Phone = "13788889999",
            VisitCount = 6,
            LastVisitAt = DateTime.UtcNow.AddDays(-7),
            Notes = "VIP老客，偏好长甲梯形款",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };
        var cust3 = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "陈嘉欣",
            Phone = "13666667777",
            VisitCount = 1,
            LastVisitAt = DateTime.UtcNow.AddDays(-15),
            Notes = "新客，对部分胶水轻微过敏，使用植物胶",
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };
        context.Customers.AddRange(cust1, cust2, cust3);

        // 7. Create Demo Appointments
        var appt1 = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = storeId,
            ServiceId = services[1].Id,
            CustomerId = cust1.Id,
            CustomerName = cust1.Name,
            CustomerPhone = cust1.Phone,
            ServiceName = services[1].Name,
            Price = services[1].Price,
            AppointmentAt = DateTime.Today.AddHours(14),
            Status = "confirmed",
            Notes = "请帮忙卸原有美甲",
            CreatedAt = DateTime.UtcNow.AddHours(-3)
        };
        var appt2 = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = storeId,
            ServiceId = services[2].Id,
            CustomerId = cust2.Id,
            CustomerName = cust2.Name,
            CustomerPhone = cust2.Phone,
            ServiceName = services[2].Name,
            Price = services[2].Price,
            AppointmentAt = DateTime.Today.AddHours(16).AddMinutes(30),
            Status = "pending",
            Notes = "想看最新的水钻款式",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        var appt3 = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = storeId,
            ServiceId = services[0].Id,
            CustomerId = cust3.Id,
            CustomerName = cust3.Name,
            CustomerPhone = cust3.Phone,
            ServiceName = services[0].Name,
            Price = services[0].Price,
            AppointmentAt = DateTime.Today.AddDays(1).AddHours(11),
            Status = "pending",
            Notes = "第一次来，准时到",
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        context.Appointments.AddRange(appt1, appt2, appt3);

        // 8. Create Demo Member Card
        var cardTemplate = new MemberCard
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "美甲美睫畅享 5 次卡",
            Type = "times",
            Price = 399m,
            TotalCount = 5,
            ValidDays = 180,
            Description = "适用于纯色跳色手部美甲或单根美睫，立省 191 元。",
            CreatedAt = DateTime.UtcNow
        };
        context.MemberCards.Add(cardTemplate);

        var custCard = new CustomerCard
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = cust2.Id,
            CardId = cardTemplate.Id,
            CardName = cardTemplate.Name,
            RemainingCount = 4,
            ExpireAt = DateTime.UtcNow.AddDays(180),
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        context.CustomerCards.Add(custCard);

        var cardLog = new CardRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerCardId = custCard.Id,
            Change = -1,
            RemainingAfter = 4,
            Remark = "到店核销美甲1次",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };
        context.CardRecords.Add(cardLog);

        // 9. Industry Templates
        context.IndustryTemplates.Add(new IndustryTemplate
        {
            Id = Guid.NewGuid(),
            Code = "BeautyNail",
            Name = "美甲美睫工作室模板",
            SeedJson = "{\"categories\":[\"精选美甲\",\"轻奢美睫\",\"面部护理\"]}"
        });
        context.IndustryTemplates.Add(new IndustryTemplate
        {
            Id = Guid.NewGuid(),
            Code = "HairSalon",
            Name = "美发沙龙理发模板",
            SeedJson = "{\"categories\":[\"总监剪发\",\"染烫设计\",\"头皮护理SPA\"]}"
        });

        await context.SaveChangesAsync();
    }
}
