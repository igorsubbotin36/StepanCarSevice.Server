using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repositories;
using System.Security.Claims;

namespace StepanCarSevice.AuthService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGeneratorService _tokenGeneratorService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserService> _logger;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        private readonly IUnitOfWork _unitOfWork;
        private TenantInfoEntity? CurrentTenant => _accessor.MultiTenantContext?.TenantInfo;

        public UserService(IUserRepository userRepository,
            IPasswordHasher passwordHasher, 
            ITokenGeneratorService tokenGeneratorService, 
            ICurrentUserService currentUserService,
            ILogger<UserService> logger,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor,
            IUnitOfWork unitOfWork
            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGeneratorService = tokenGeneratorService;
            _currentUserService = currentUserService;
            _logger = logger;
            _accessor = accessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || !BelongsToCurrentTenant(user))
                return Result.Failure(UserErrors.NotFound);
            if (!_passwordHasher.Verify(request.OldPassword, user.Password))
            {
                _logger.LogWarning($"{user.Id} неверный текущий пароль при попытке смены пароля");
                return Result.Failure(UserErrors.WrongPassword);
            }
            user.Password = _passwordHasher.Hash(request.NewPassword);
            try
            {
                await _userRepository.UpdateUserAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"{user.Id} пароль сменен");
                return Result.Success();
            }
            catch (Exception e) 
            {
                _logger.LogError($"{user.Id} ошибка при попытке смены пароля\n{e}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
            
        }

        public async Task<Result> UpdateUserAsync(EditUserRequestDto request, int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || !BelongsToCurrentTenant(user))
                return Result.Failure(UserErrors.NotFound);
            if (request.Phone != null && request.Phone != user.Phone
                && await _userRepository.ExistsByPhoneAsync(request.Phone, user.TenantId))
                return Result.Failure(RegisterErrors.UserAlreadyExists);
            if (request.FirstName != null)
                user.FirstName = request.FirstName;
            if (request.SecondName != null)
                user.SecondName = request.SecondName;
            if (request.Email != null)
                user.Email = request.Email;
            if (request.Phone != null)
                user.Phone = request.Phone;
            try
            {
                await _userRepository.UpdateUserAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"{user.Id} информация обновлена");
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError($"{user.Id} ошибка при попытке обновления информации о пользователе\n{e}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }
        // Пользователь существует только в своём тенанте: не полагаемся лишь на проверку tenant_id в JWT
        private bool BelongsToCurrentTenant(User user) =>
            user.Role?.Name == "GodMode" || user.TenantId == GetTenantId();

        private string? GetTenantId()
        {
            var tenant = CurrentTenant;
            if (tenant == null)
                return null;
            return tenant.Id;
        }

        public async Task<Result<UserInfoDto>> GetUserInfoByPhoneAsync(string phone)
        {
            User? person = await _userRepository.GetUserByPhoneAsync(phone, GetTenantId());
            if (person == null)
                return Result.Failure<UserInfoDto>(UserErrors.InvalidPhone);
            UserInfoDto userInfoDto = new UserInfoDto(person.FirstName, person.SecondName, person.Email, person.Phone, person.Role.Name);
            return Result.Success(userInfoDto);
        }

        public async Task<Result<List<UserInfoDto>>> GetAllUsersAsync()
        {
            var list = await _userRepository.GetAllUsersAsync(GetTenantId());
            List<UserInfoDto> result = new List<UserInfoDto>();
            foreach (var person in list)
            {
                UserInfoDto userInfoDto = new UserInfoDto(person.FirstName, person.SecondName, person.Email, person.Phone, person.Role.Name);
                result.Add(userInfoDto);
            }
            return Result.Success(result);
        }
    }
}
