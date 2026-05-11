using Microsoft.EntityFrameworkCore;
using StepanCarService.Core.Entities;
using StepanCarService.Web.DbContexts;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Infrastructure.DBContexts
{
    public class DetailDbContext : TenantBaseDbContext
    {
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<CarManufacture> CarManufactures { get; set; }
        public DbSet<DetailManufacture> DetailManufactures { get; set; }

        public DetailDbContext(DbContextOptions<DetailDbContext> options) : base(options) { }
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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DetailDbContext).Assembly);
        }
    }
}
