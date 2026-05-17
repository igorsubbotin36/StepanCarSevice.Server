using StepanCarSevice.AuthService.Domain.Entities;

namespace StepanCarSevice.AuthService.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByPhoneAsync(string phone);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByPhoneAsync(string phone);
        Task<int?> GetRoleIdAsync(string name);
    }
}
