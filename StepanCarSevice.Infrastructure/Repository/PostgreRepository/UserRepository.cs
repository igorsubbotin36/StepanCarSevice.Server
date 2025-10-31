using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StepanCarSevice.Infrastructure.DbContexts;
using StepanCarSevice.Domain.Entities;
using StepanCarSevice.Application.Models.Dto;
using StepanCarSevice.Application.Repository.Interfaces;
using StepanCarSevice.Application.Auth;

namespace StepanCarSevice.Infrastructure.Repository.PostgreRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly Microsoft.Extensions.Logging.ILogger<UserRepository> _logger;
        private readonly PostgreDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        public UserRepository(Microsoft.Extensions.Logging.ILogger<UserRepository> logger, PostgreDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }
        public async Task<bool> AddUser(RegisterDto model)
        {
            try
            {
                User newUser = new User() { Email = model.Email, 
                    Password = _passwordHasher.Hash(model.Password), 
                    Role = "user", 
                    FirstName = model.FirstName,
                    SecondName = model.SecondName,
                    Phone = model.Phone};
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($@"Пользователь {newUser.Email} зарегистрирован!");
                return true;
            }
            catch(Exception e)
            {
                _logger.LogError($@"Ошибка при регистрации пользователя {model.Email}", e);
                return false;
            }
        }
        public async Task<User?> GetUserById(int id)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }
      
        public async Task<bool> DeleteUser(string phone)
        {
            try
            {
                User? user = await GetUserByPhone(phone);
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($@"Пользователь с номером телефона {phone} удален!");
                    return true;
                }
                else
                {
                    _logger.LogWarning($@"Пользователь с номером телефона {phone} не найден!");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($@"Ошибка удаления пользователя с номером телефона {phone}", e);
                return false;
            }
        }
        private async Task<User> EditModelToUser(EditUserDto editModel)
        {
            User? user = await GetUserByPhone(editModel.OldPhone);
            if (user is null)
            {
                throw new InvalidOperationException($"User with phone {editModel.OldPhone} not found");
            }
            user.Email = editModel.Email != null ? editModel.Email : user.Email;
            user.Phone = editModel.Phone != null ? editModel.Phone : user.Phone;
            user.FirstName = editModel.FirstName != null ? editModel.FirstName : user.FirstName;
            user.SecondName = editModel.SecondName != null ? editModel.SecondName : user.SecondName;
            return user;
        }
        public async Task<bool> EditUser(EditUserDto editModel)
        {
            try
            {
                User user = await EditModelToUser(editModel);
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($@"У пользователя {user.Id} изменилась информация");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($@"Ошибка изменения информации у пользователя с email {editModel.Email}", e);
                return false;
            }
        }
        public async Task<bool> ChangePassword(string password, string phone)
        {
            try
            {
                User? user = await GetUserByPhone(phone);
                if (user is null)
                {
                    _logger.LogWarning($"Пользователь с номером телефона {phone} не найден");
                    return false;
                }
                user.Password = _passwordHasher.Hash(password);
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($@"Пользователь с номером телефона {phone} сменил пароль");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $@"Пользователь с номером телефона {phone} не смог сменить пароль");
                return false;
            }
        }
        public async Task<List<User>> GetAllUsers()
        {
            List<User> users = await _dbContext.Users.ToListAsync();
            return users;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user;
        }
        public async Task<User?> GetUserByPhone(string phone)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Phone == phone);
            return user;
        }
    }
}
