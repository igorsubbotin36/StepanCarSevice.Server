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
            var person = await FindUserForLoginAsync(request.Phone, request.Password);

            if (person == null)
            {
                return Result.Failure<AuthResponseDto>(AuthErrors.InvalidCredentials);
            }
            var tenant = CurrentTenant;
            if (tenant != null && !tenant.IsActive && person.Role.Name != Roles.GodMode)
            {
                return Result.Failure<AuthResponseDto>(TenantErrors.TenantInactive);
            }
            var token = _tokenGeneratorService.GenerateToken(BuildIdentity(person));
            return Result.Success(token);
        }
        public async Task<Result> RegisterAsync(RegisterRequestDto request)
        {
            if (request.Password != request.ConfirmPassword)
                return Result.Failure(RegisterErrors.PasswordsDontMatch);

            if (CurrentTenant is { IsActive: false })
                return Result.Failure(TenantErrors.TenantInactive);

            if (await _userRepository.ExistsByPhoneAsync(request.Phone, GetTenantId()))
                return Result.Failure(RegisterErrors.UserAlreadyExists);

            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = new User()
            {
                Email = request.Email,
                Phone = request.Phone,
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                Name = $"{request.FirstName} {request.SecondName}",
                Password = passwordHash
            };

            var tenantId = GetTenantId();
            int? userRoleId;
            if (tenantId == null)
            {
                // Регистрация на портале — это владелец автосервиса
                userRoleId = await _userRepository.GetRoleIdAsync(Roles.TenantOwner);
                if (userRoleId == null)
                    return Result.Failure(RegisterErrors.RoleIdNotFound);
                user.TenantId = null;
            }
            else
            {
                userRoleId = await _userRepository.GetRoleIdAsync(Roles.User);
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

        // Портал: только GodMode и TenantOwner.
        // Поддомен тенанта: пользователь этого тенанта, либо GodMode, либо владелец именно этого тенанта с логином портала
        private async Task<User?> FindUserForLoginAsync(string phone, string password)
        {
            var tenant = CurrentTenant;

            if (tenant != null)
            {
                var tenantUser = await _userRepository.GetUserByPhoneAsync(phone, tenant.Id);
                if (tenantUser != null && _passwordHasher.Verify(password, tenantUser.Password))
                    return tenantUser;
            }

            var portalUser = await _userRepository.GetUserByPhoneAsync(phone, null);
            if (portalUser == null || !_passwordHasher.Verify(password, portalUser.Password))
                return null;

            var role = portalUser.Role.Name;
            if (tenant == null)
                return role is Roles.GodMode or Roles.TenantOwner ? portalUser : null;
            if (role == Roles.GodMode)
                return portalUser;
            if (role == Roles.TenantOwner && tenant.OwnerUserId == portalUser.Id)
                return portalUser;
            return null;
        }

        private static ClaimsIdentity BuildIdentity(User person)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, person.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, person.Phone ?? string.Empty),
                new Claim(ClaimTypes.Role, person.Role.Name),
                new Claim(ClaimTypes.Email, person.Email),
                new Claim(ClaimTypes.GivenName, person.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, person.SecondName ?? string.Empty)
            };
            // У пользователей портала tenant_id нет: права владельца в тенанте проверяются при каждом запросе
            if (!string.IsNullOrEmpty(person.TenantId))
            {
                claims.Add(new Claim("tenant_id", person.TenantId));
            }
            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
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
