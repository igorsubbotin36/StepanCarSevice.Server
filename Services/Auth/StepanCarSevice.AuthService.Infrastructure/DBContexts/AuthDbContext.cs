using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarSevice.AuthService.Domain.Entities;

namespace StepanCarSevice.AuthService.Infrastructure.DBContexts
{
    public class AuthDbContext : TenantBaseDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

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

            // Пользователи портала (GodMode, TenantOwner) не принадлежат тенанту.
            // Пользователи тенанта удаляются вместе с ним, поэтому «осиротевших» учёток на портале не бывает
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        }
    }
}
