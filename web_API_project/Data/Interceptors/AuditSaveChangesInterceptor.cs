using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace web_API_project.Data.Interceptors
{
    public class AuditSaveChangesInterceptor(ILogger<AuditSaveChangesInterceptor> logger) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            LogChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            LogChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void LogChanges(DbContext? context)
        {
            if (context is null) return;
            foreach (var entry in context.ChangeTracker.Entries().Where(entry =>
                         entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
                logger.LogInformation("EF Core {State}: {EntityType}", entry.State, entry.Metadata.ClrType.Name);
        }
    }
}
