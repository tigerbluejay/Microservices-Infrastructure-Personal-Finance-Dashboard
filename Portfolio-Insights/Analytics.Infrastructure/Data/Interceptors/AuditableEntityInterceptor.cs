using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Analytics.Infrastructure.Data.Interceptors
{
    /// <summary>
    /// Stub interceptor for auditing (created/modified timestamps etc.).
    /// Extend if you add IAuditableEntity in domain.
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
    }
}