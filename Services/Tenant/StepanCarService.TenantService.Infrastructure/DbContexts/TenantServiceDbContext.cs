using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarService.TenantService.Infrastructure.Messaging.Outbox;

namespace StepanCarService.TenantService.Infrastructure.DbContexts;

public class TenantServiceDbContext : TenantBaseDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public TenantServiceDbContext(DbContextOptions<TenantServiceDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            optionsBuilder.UseNpgsql(connectionString);
        }
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // У владельца может быть только один тенант (NULL — тенанты GodMode — не ограничены)
        modelBuilder.Entity<TenantInfoEntity>()
            .HasIndex(t => t.OwnerUserId)
            .IsUnique();

        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable("OutboxMessages");
            outbox.HasKey(m => m.Id);
            outbox.Property(m => m.Id).UseIdentityAlwaysColumn();
            outbox.Property(m => m.EventType).HasMaxLength(200);
            outbox.Property(m => m.LastError).HasMaxLength(2000);
            outbox.HasIndex(m => m.MessageId).IsUnique();
            // Неотправленные события — в порядке публикации
            outbox.HasIndex(m => m.Id)
                .HasDatabaseName("IX_OutboxMessages_Pending")
                .HasFilter("\"ProcessedAt\" IS NULL");
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantServiceDbContext).Assembly);
    }
}