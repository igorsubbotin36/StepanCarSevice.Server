using StepanCarSevice.Domain.Entities;
using StepanCarSevice.Application.Models.Dto;

namespace StepanCarSevice.Application.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByEmail(string email);
        public Task<User?> GetUserByPhone(string phone);
        public Task<List<User>> GetAllUsers();
        public Task<User?> GetUserById(int id);
        public Task<bool> AddUser(RegisterDto model);
        public Task<bool> EditUser(EditUserDto editModel);
        public Task<bool> DeleteUser(string phone);
        public Task<bool> ChangePassword(string password, string phone);
    }
}
