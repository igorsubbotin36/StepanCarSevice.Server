using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using StepanCarService.Common.Application.Models;
using StepanCarService.Common.Core.Entities;
using StepanCarService.Common.Core.Repositories;
using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repositories;
using System.Security.Claims;

namespace StepanCarSevice.AuthService.Application.Services
{
    public class AuthorizationService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGeneratorService _tokenGeneratorService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IMultiTenantContextAccessor<TenantInfoEntity> _accessor;
        private readonly IUnitOfWork _unitOfWork;
        public AuthorizationService(IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenGeneratorService tokenGeneratorService,
            ICurrentUserService currentUserService,
            ILogger<AuthorizationService> logger,
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
        private TenantInfoEntity? CurrentTenant => _accessor.MultiTenantContext?.TenantInfo;
        public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var identity = await GetIdentityAsync(request.Phone, request.Password);

            if (identity == null)
            {
                return Result.Failure<AuthResponseDto>(AuthErrors.InvalidCredentials);
            }
            var token = _tokenGeneratorService.GenerateToken(identity);
            return Result.Success(token);
        }
        public async Task<Result> RegisterAsync(RegisterRequestDto request)
        {
            if (request.Password != request.ConfirmPassword)
                return Result.Failure(RegisterErrors.PasswordsDontMatch);

            if (await _userRepository.ExistsByPhoneAsync(request.Phone, GetTenantId()))
                return Result.Failure(RegisterErrors.UserAlreadyExists);

            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = new User()
            {
                Email = request.Email,
                Phone = request.Phone,
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                Password = passwordHash
            };

            var tenantId = GetTenantId();
            int? userRoleId;
            if (tenantId == null)
            {
                userRoleId = await _userRepository.GetRoleIdAsync("TenantOwner");
                if (userRoleId == null)
                    return Result.Failure(RegisterErrors.RoleIdNotFound);
                user.TenantId = null;
            }
            else
            {
                userRoleId = await _userRepository.GetRoleIdAsync("User");
                if (userRoleId == null)
                    return Result.Failure(RegisterErrors.RoleIdNotFound);
                user.TenantId = tenantId;
            }
            user.RoleId = (int)userRoleId;
            try
            {
                await _userRepository.AddUserAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"{user.Phone} зарегистрирован");
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError($"Ошибка БД при регистрации пользователя {user.Phone}\n{e}");
                return Result.Failure(SystemErrors.DatabaseError);
            }
        }

        private async Task<ClaimsIdentity?> GetIdentityAsync(string phone, string password)
        {
            var tenantId = GetTenantId();
            User? person = await _userRepository.GetUserByPhoneAsync(phone, tenantId);
            if (person == null)
            {
                person = await _userRepository.GetUserByPhoneAsync(phone, null);
                if (person != null && person.Role.Name != "GodMode")
                    return null;
            }
            if (person != null && _passwordHasher.Verify(password, person.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.MobilePhone, person.Phone ?? string.Empty),
                    new Claim(ClaimTypes.Role, person.Role.Name),
                    new Claim(ClaimTypes.Email, person.Email),
                    new Claim(ClaimTypes.GivenName, person.FirstName),
                    new Claim(ClaimTypes.Surname, person.SecondName)
                };
                if (!string.IsNullOrEmpty(person.TenantId))
                {
                    claims.Add(new Claim("tenant_id", person.TenantId));
                }
                return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }
            return null;
        }
        private string? GetTenantId()
        {
            var tenant = CurrentTenant;
            if (tenant == null)
                return null;
            return tenant.Id;
        }
        public Result<UserDto> GetClaims()
        {
            var claims = _currentUserService.GetAllClaims();
            if (claims == null || claims.Count == 0)
                return Result.Failure<UserDto>(AuthErrors.InvalidCredentials);
            List<ClaimDto> list = new List<ClaimDto>();
            var excludedTypes = new HashSet<string>
            {
                "nbf", "exp", "iss", "aud"
            };
            foreach (var claim in claims)
            {
                if (excludedTypes.Contains(claim.Type))
                    continue;
                list.Add(new ClaimDto(claim.Type.Split('/').Last(), claim.Value));
            }
            UserDto result = new UserDto(list);
            return Result.Success(result);
        }
    }
}
