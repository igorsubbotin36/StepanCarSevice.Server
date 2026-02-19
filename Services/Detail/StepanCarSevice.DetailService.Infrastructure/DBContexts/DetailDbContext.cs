using Microsoft.EntityFrameworkCore;
using StepanCarSevice.DetailService.Domain.Entities;

namespace StepanCarSevice.DetailService.Infrastructure.DBContexts
{
    public class DetailDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Manufacture> Manufactures { get; set; }
        public DbSet<User> Users { get; set; }

        public DetailDbContext(DbContextOptions<DetailDbContext> options) : base(options) { }
        public DetailDbContext() { }
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
