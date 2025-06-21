using Microsoft.EntityFrameworkCore;
using StepanCarSevice.Server.Entities;

namespace StepanCarSevice.Server.DbContexts
{
    public partial class PostgreDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarModel> Car1Models { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Manufacture> Manufactures { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public PostgreDbContext() { }
    }
}
