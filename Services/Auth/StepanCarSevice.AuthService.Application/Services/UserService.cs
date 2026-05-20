using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
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
        private TenantInfoEntity? CurrentTenant => _accessor.MultiTenantContext?.TenantInfo;

        public UserService(IUserRepository userRepository,
            IPasswordHasher passwordHasher, 
            ITokenGeneratorService tokenGeneratorService, 
            ICurrentUserService currentUserService,
            ILogger<UserService> logger,
            IMultiTenantContextAccessor<TenantInfoEntity> accessor
            )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGeneratorService = tokenGeneratorService;
            _currentUserService = currentUserService;
            _logger = logger;
            _accessor = accessor;
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, string phone)
        {
            if (phone == null)
                return Result.Failure(AuthErrors.InvalidCredentials);
            var user = await _userRepository.GetUserByPhoneAsync(phone, GetTenantId());
            if (user == null) 
                return Result.Failure(UserErrors.InvalidPhone);
            user.Password = _passwordHasher.Hash(request.NewPassword);
            try
            {
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation($"{user.Id} пароль сменен");
                return Result.Success();
            }
            catch (Exception e) 
            {
                _logger.LogError($"{user.Id} ошибка при попытке смены пароля\n{e}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
            
        }

        public async Task<Result> UpdateUserAsync(EditUserRequestDto request, string phone)
        {
            if (phone == null)
                return Result.Failure(AuthErrors.InvalidCredentials);
            var user = await _userRepository.GetUserByPhoneAsync(phone, GetTenantId());
            if (user == null)
                return Result.Failure(UserErrors.InvalidPhone);
            if (request.FirstName != null)
                user.FirstName = request.FirstName;
            if (request.SecondName != null)
            user.SecondName = request.SecondName;
            if (request.Email != null)
                user.Email = request.Email;
            if (request.Phone != null)
                user.Phone = request.Phone;
            if (request.TenantId != null)
                user.TenantId = request.TenantId;
            try
            {
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation($"{user.Id} информация обновлена");
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError($"{user.Id} ошибка при попытке обновления информации о пользователе\n{e}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }
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
