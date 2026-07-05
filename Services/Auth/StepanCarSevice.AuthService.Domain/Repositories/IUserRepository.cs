using StepanCarSevice.AuthService.Domain.Entities;

namespace StepanCarSevice.AuthService.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email, string? tenantId);
        Task<User?> GetUserByPhoneAsync(string phone, string? tenantId);
        Task<User?> GetUserByIdAsync(int id, string? tenantId);
        Task<List<User>> GetAllUsersAsync(string? tenantId);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<bool> ExistsByEmailAsync(string email, string? tenantId);
        Task<bool> ExistsByPhoneAsync(string phone, string? tenantId);
        Task<int?> GetRoleIdAsync(string name);
    }
}
