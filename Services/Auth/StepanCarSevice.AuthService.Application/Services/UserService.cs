using StepanCarService.Core.Models;
using StepanCarSevice.AuthService.Application.Auth;
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

        public UserService(IUserRepository userRepository,
            IPasswordHasher passwordHasher, 
            ITokenGeneratorService tokenGeneratorService, 
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGeneratorService = tokenGeneratorService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequestDto request, string phone)
        {
            if (phone == null)
                return Result.Failure(AuthErrors.InvalidCredentials);
            var user = await _userRepository.GetUserByPhoneAsync(phone);
            if (user == null) 
                return Result.Failure(UserErrors.InvalidPhone);
            user.Password = _passwordHasher.Hash(request.NewPassword);
            if (!(await _userRepository.UpdateUserAsync(user)))
                return Result.Failure("DATABASE_ERROR");
            return Result.Success();
        }
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

            if (await _userRepository.ExistsByPhoneAsync(request.Phone))
                return Result.Failure(RegisterErrors.UserAlreadyExists);

            int? userRoleId = await _userRepository.GetRoleIdAsync("User");
            if (userRoleId == null)
                return Result.Failure(RegisterErrors.RoleIdNotFound);

            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = new User() { 
                Email = request.Email,
                Phone = request.Phone,
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                Password = passwordHash,
                RoleId = (int)userRoleId
            };

            await _userRepository.AddUserAsync(user);

            return Result.Success();
        }

        public Result<UserDto> GetClaims()
        {
            var claims = _currentUserService.GetAllClaims();
            if (claims == null || claims.Count == 0)
                return Result.Failure<UserDto>(AuthErrors.InvalidCredentials);
            UserDto result = new UserDto();
            result.Claims = new List<ClaimDto>();
            var excludedTypes = new HashSet<string>
            {
                "nbf", "exp", "iss", "aud"
            };
            foreach (var claim in claims)
            {
                if (excludedTypes.Contains(claim.Type))
                    continue;
                result.Claims.Add(new ClaimDto() { Type = claim.Type.Split('/').Last(), Value = claim.Value });
            }
            return Result.Success(result);
        }

        public async Task<Result> UpdateUserAsync(EditUserRequestDto request, string phone)
        {
            if (phone == null)
                return Result.Failure(AuthErrors.InvalidCredentials);
            var user = await _userRepository.GetUserByPhoneAsync(phone);
            if (user == null)
                return Result.Failure(UserErrors.InvalidPhone);
            user.FirstName = request.FirstName;
            user.SecondName = request.SecondName;
            if (request.Email != null)
                user.Email = request.Email;
            if (request.Phone != null)
                user.Phone = request.Phone;
            if (!(await _userRepository.UpdateUserAsync(user)))
                return Result.Failure("DATABASE_ERROR");
            return Result.Success();
        }

        public async Task<ClaimsIdentity?> GetIdentityAsync(string phone, string password)
        {
            User? person = await _userRepository.GetUserByPhoneAsync(phone);
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
                return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }
            return null;
        }

        public async Task<Result<UserInfoDto>> GetUserInfoByPhoneAsync(string phone)
        {
            User? person = await _userRepository.GetUserByPhoneAsync(phone);
            if (person == null)
                return Result.Failure<UserInfoDto>(UserErrors.InvalidPhone);
            UserInfoDto userInfoDto = new UserInfoDto() { 
                Email = person.Email, 
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                Phone = person.Phone,
                Role = person.Role.Name
            };
            return Result.Success(userInfoDto);
        }
    }
}
