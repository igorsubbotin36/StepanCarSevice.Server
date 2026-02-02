using StepanCarSevice.AuthService.Application.Auth;
using StepanCarSevice.AuthService.Application.Interfaces.Services;
using StepanCarSevice.AuthService.Application.Models;
using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Entities;
using StepanCarSevice.AuthService.Domain.Repository;
using System.Security.Claims;

namespace StepanCarSevice.AuthService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGeneratorService _tokenGeneratorService;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenGeneratorService tokenGeneratorService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGeneratorService = tokenGeneratorService;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            throw new NotImplementedException();
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
                //throw new ValidationException("Пароли не совпадают");
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

        public Result<UserDto> GetClaims(string token)
        {
            var list = _tokenGeneratorService.ValidateToken(token);
            if (list==null || list.Count == 0)
                return Result.Failure<UserDto>(AuthErrors.TokenIsNotValid);
            var excludedTypes = new HashSet<string>
            {
                "nbf", "exp", "iss", "aud"
            };

            UserDto result = new UserDto();
            result.Claims = new List<ClaimDto>();
            foreach (var claim in list)
            {
                if (excludedTypes.Contains(claim.Type))
                    continue;
                result.Claims.Add(new ClaimDto() { Type = claim.Type.Split('/').Last(), Value = claim.Value });
            }
            return Result.Success<UserDto>(result);
        }

        public async Task<bool> UpdateUserAsync(int userId, EditUserRequestDto request)
        {
            throw new NotImplementedException();
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
                    new Claim("FirstName", person.FirstName),
                    new Claim("SecondName", person.SecondName)
                };
                return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }
            return null;
        }
    }
}
