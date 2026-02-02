using StepanCarSevice.AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Infrastructure.DBContexts.Inits
{
    public static class DbInitializer
    {
        public static void Init(AuthDbContext dbContext)
        {
            if (!dbContext.Roles.Any())
            {

                var roles = new List<Role>
                {
                new Role{ Id = 1, Name = "Admin", Description = "Администратор" },
                new Role{ Id = 2, Name = "User", Description = "Пользователь" }
                };
                dbContext.Roles.AddRange(roles);
                dbContext.SaveChanges();
            }
        }
    }
}
