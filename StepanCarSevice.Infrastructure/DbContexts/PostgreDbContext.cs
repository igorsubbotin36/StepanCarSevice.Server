using Microsoft.EntityFrameworkCore;
using StepanCarSevice.Domain.Entities;

namespace StepanCarSevice.Infrastructure.DbContexts
{
    public partial class PostgreDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Manufacture> Manufactures { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Work> Works { get; set; }
        public PostgreDbContext(DbContextOptions<PostgreDbContext> opt) : base(opt)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // использование Fluent API
            base.OnModelCreating(modelBuilder);
        }

    }
}
