using Microsoft.AspNetCore.Mvc;
using StepanCarSevice.Server.Entities;
using StepanCarSevice.Server.Models;

namespace StepanCarSevice.Server.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByEmail(string email);
        public Task<User?> GetUserByPhone(string phone);
        public Task<List<User>> GetAllUsers();
        public Task<User?> GetUserById(int id);
        public Task<bool> AddUser(RegisterModel model);
        public Task<bool> EditUser(EditUserModel editModel);
        public Task<bool> DeleteUser(string email);
        public Task<bool> ChangePassword(string password, string email);
    }
}
