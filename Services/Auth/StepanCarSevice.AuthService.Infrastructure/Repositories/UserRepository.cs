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
            await _dbContext.SaveChangesAsync();
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }
        public async Task DeleteUserAsync(User user)
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = await _dbContext.Users
                .Include(x => x.Role)
                .ToListAsync();
            return users;
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            User? user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
            return user;
        }
        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            User? user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Phone == phone);
            return user;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return false;
            return true;
        }

        public async Task<bool> ExistsByPhoneAsync(string phone)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Phone == phone);
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
