using Microsoft.EntityFrameworkCore;
using StepanCarService.Common.Infastructure.DbContexts;
using StepanCarSevice.VisitService.Domain.Entities;

namespace StepanCarSevice.VisitService.Infrastructure.DBContexts
{
    public class VisitDBContext : TenantBaseDbContext
    {
        public DbSet<CarSnapshot> CarSnapshots { get; set; }
        public DbSet<CarModelSnapshot> CarModelSnapshots { get; set; }
        public DbSet<DetailSnapshot> DetailSnapshots { get; set; }
        public DbSet<ManufactureSnapshot> ManufactureSnapshots { get; set; }
        public DbSet<OwnerSnapshot> OwnerSnapshots { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<VisitDetails> VisitDetails { get; set; }

        public VisitDBContext(DbContextOptions<VisitDBContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

                object value = optionsBuilder.UseNpgsql(connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Work>()
                .HasOne(w => w.Visit)
                .WithMany(v => v.Works)
                .HasForeignKey(w => w.VisitId);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VisitDBContext).Assembly);
        }
    }
}
