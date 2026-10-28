using MiniStore.Api.Services;

namespace MiniStore.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // 1. Try to read tenant_id from Claims
        var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantIdFromClaim))
        {
            tenantContext.SetTenantId(tenantIdFromClaim);
        }
        // 2. Alternatively check X-Tenant-Id header
        else if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
                 Guid.TryParse(tenantHeader, out var tenantIdFromHeader))
        {
            tenantContext.SetTenantId(tenantIdFromHeader);
        }

        await _next(context);
    }
}
