using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repositories;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarSevice.AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _dbContext;
        public UserRepository(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddUserAsync(User user)
        {
            _dbContext.Users.Add(user);
        }
        public async Task<User?> GetUserByIdAsync(int id, string? tenantId)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            return user;
        }
        public async Task DeleteUserAsync(User user)
        {
            _dbContext.Users.Remove(user);
        }
        public async Task UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);
        }
        public async Task<List<User>> GetAllUsersAsync(string? tenantId)
        {
            List<User> users = await _dbContext.Users
                .Where(x => x.TenantId == tenantId)
                .Include(x => x.Role)
                .ToListAsync();
            return users;
        }
        public async Task<User?> GetUserByEmailAsync(string email, string? tenantId)
        {
            User? user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email && x.TenantId == tenantId);
            return user;
        }
        public async Task<User?> GetUserByPhoneAsync(string phone, string? tenantId)
        {
            User? user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Phone == phone && x.TenantId == tenantId);
            return user;
        }

        public async Task<bool> ExistsByEmailAsync(string email, string? tenantId)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.TenantId == tenantId);
            if (user == null) return false;
            return true;
        }

        public async Task<bool> ExistsByPhoneAsync(string phone, string? tenantId)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Phone == phone && x.TenantId == tenantId);
            if (user == null) return false;
            return true;
        }

        public async Task<int?> GetRoleIdAsync(string name)
        {
            Role? role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == name);
            if (role == null) return null;
            return role.Id;
        }
    }
}
