using System.Linq.Expressions;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Exceptions;

namespace StepanCarService.Common.Infastructure.DbContexts
{
    // Контекст сервиса с данными тенантов (Detail, Visit).
    // Все сущности ITenantScoped:
    //  - читаются только в тенанте текущего запроса (поддомен); на портале и в фоновых задачах — пусто;
    //  - при создании получают TenantId текущего тенанта;
    //  - не могут быть созданы, изменены или удалены вне своего тенанта.
    public abstract class TenantScopedDbContext : TenantBaseDbContext
    {
        private readonly IMultiTenantContextAccessor<TenantInfoEntity>? _tenantAccessor;

        protected TenantScopedDbContext(DbContextOptions options,
            IMultiTenantContextAccessor<TenantInfoEntity>? tenantAccessor) : base(options)
        {
            _tenantAccessor = tenantAccessor;
        }

        // Тенант текущего запроса; null — портал, фоновые задачи, миграции
        public string? CurrentTenantId => _tenantAccessor?.MultiTenantContext?.TenantInfo?.Id;

        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureModel(modelBuilder);
            ApplyTenantQueryFilters(modelBuilder);
        }

        // Конфигурация модели конкретного сервиса (вместо OnModelCreating)
        protected abstract void ConfigureModel(ModelBuilder modelBuilder);

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            EnforceTenantOnSave();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            EnforceTenantOnSave();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Фильтр задаётся на корневой тип иерархии
                if (entityType.BaseType != null || !typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                    continue;

                // e => CurrentTenantId != null && e.TenantId == CurrentTenantId
                // Обращение к свойству контекста EF параметризует и вычисляет для каждого запроса
                var entity = Expression.Parameter(entityType.ClrType, "e");
                var currentTenantId = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                var body = Expression.AndAlso(
                    Expression.NotEqual(currentTenantId, Expression.Constant(null, typeof(string))),
                    Expression.Equal(Expression.Property(entity, nameof(ITenantScoped.TenantId)), currentTenantId));
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, entity));
            }
        }

        private void EnforceTenantOnSave()
        {
            var currentTenantId = CurrentTenantId;
            foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
            {
                var entityName = entry.Metadata.ClrType.Name;
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (currentTenantId == null)
                            throw new TenantAccessViolationException($"{entityName}: данные тенанта нельзя создать вне тенанта.");
                        if (string.IsNullOrEmpty(entry.Entity.TenantId))
                            entry.Entity.TenantId = currentTenantId;
                        else if (entry.Entity.TenantId != currentTenantId)
                            throw new TenantAccessViolationException($"{entityName}: запись в чужой тенант.");
                        break;

                    case EntityState.Modified:
                    case EntityState.Deleted:
                        var originalTenantId = entry.Property(nameof(ITenantScoped.TenantId)).OriginalValue as string;
                        if (currentTenantId == null
                            || originalTenantId != currentTenantId
                            || entry.Entity.TenantId != currentTenantId)
                            throw new TenantAccessViolationException($"{entityName}: изменение данных чужого тенанта.");
                        break;
                }
            }
        }
    }
}
