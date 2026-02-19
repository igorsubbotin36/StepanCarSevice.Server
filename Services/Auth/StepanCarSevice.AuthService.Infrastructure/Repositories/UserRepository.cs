using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repositories;
using StepanCarSevice.AuthService.Infrastructure.DBContexts;

namespace StepanCarSevice.AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly AuthDbContext _dbContext;
        public UserRepository(ILogger<UserRepository> logger, AuthDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Пользователь {user.Email} зарегистрирован!");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка при добавлении пользователя {user.Email} в БД", e);
                return false;
            }
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }
        public async Task<bool> DeleteUserAsync(User user)
        {
            try
            {
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Пользователь с номером телефона {user.Phone} удален!");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Пользователь с номером телефона {user.Phone} не найден!");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка удаления пользователя с номером телефона {user.Phone}", e);
                return false;
            }
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"У пользователя {user.Id} изменилась информация");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка изменения информации у пользователя с email {user.Email}", e);
                return false;
            }
        }
        public async Task<bool> ChangePasswordAsync(string password, string phone)
        {
            try
            {
                User? user = await GetUserByPhoneAsync(phone);
                if (user is null)
                {
                    _logger.LogWarning($"Пользователь с номером телефона {phone} не найден");
                    return false;
                }
                user.Password = password;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Пользователь с номером телефона {phone} сменил пароль");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Пользователь с номером телефона {phone} не смог сменить пароль");
                return false;
            }
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
