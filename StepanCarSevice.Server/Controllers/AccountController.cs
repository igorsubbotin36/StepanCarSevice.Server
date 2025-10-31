using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StepanCarSevice.Application.Auth;
using StepanCarService.Server.Auth;
using StepanCarSevice.Domain.Entities;
using StepanCarSevice.Application.Repository.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Text;

using StepanCarSevice.Application.Models.Dto;

namespace StepanCarService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserRepository _rep;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtOptions _jwtOptions;
        public AccountController(IUserRepository rep, ILogger<AccountController> logger, IPasswordHasher passwordHasher, IOptions<JwtOptions> jwtOptions)
        {
            _logger = logger;
            _rep = rep;
            _passwordHasher = passwordHasher;
            _jwtOptions = jwtOptions.Value;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (!string.IsNullOrEmpty(model.Phone) && await _rep.GetUserByPhone(model.Phone) != null)
            {
                return Conflict(new { errorText = "Пользователь с таким телефоном уже существует" });
            }
            else if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { errorText = "Пароли не совпадают" });
            }
            if (await _rep.AddUser(model))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, new { errorText = "Произошла ошибка при регистрации" });
            }
        }
        [HttpDelete("deleteUser")]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> DeleteUser([FromQuery] string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return BadRequest(new { errorText = "Телефон обязателен" });
            string? role = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return Unauthorized();
            if (role == "admin")
            {
                var user = await _rep.GetUserByPhone(phone);
                if (user == null)
                {
                    return NotFound(new { errorText = "Пользователь не найден" });
                }
                if (await _rep.DeleteUser(phone))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, new { errorText = "Не удалось удалить пользователя" });
                }
            }
            else
            {
                string? phoneUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
                if (string.IsNullOrEmpty(phoneUser)) return Unauthorized();
                if (phone == phoneUser)
                {
                    var user = await _rep.GetUserByPhone(phone);
                    if (user == null)
                    {
                        return NotFound(new { errorText = "Пользователь не найден" });
                    }
                    if (await _rep.DeleteUser(phone))
                    {
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(500, new { errorText = "Не удалось удалить пользователя" });
                    }
                }
                else
                {
                    return Forbid();
                }
            }
        }

        [HttpGet("getAllUsers")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _rep.GetAllUsers();
            var result = users.Select(u => new UserDto { Id = u.Id, FirstName = u.FirstName, SecondName = u.SecondName, Email = u.Email, Phone = u.Phone ?? string.Empty, Role = u.Role }).ToList();
            return Ok(result);
        }

        [HttpPatch("changePassword")]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordModel)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            string? phoneUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
            if (string.IsNullOrEmpty(phoneUser)) return Unauthorized();
            if (passwordModel.OldPassword != null 
                && passwordModel.NewPassword != null
                && passwordModel.ConfirmPassword != null
                && passwordModel.NewPassword == passwordModel.ConfirmPassword)
            {
                var user = await _rep.GetUserByPhone(phoneUser);
                if (user != null && _passwordHasher.Verify(passwordModel.OldPassword, user.Password)
                    && await _rep.ChangePassword(passwordModel.NewPassword, phoneUser))
                {
                    return Ok();
                }
                else
                {
                return BadRequest(new { errorText = "Не удалось сменить пароль" });
                }
            }
            else
            {
                return BadRequest(new { errorText = "Пароли не совпадают" });
            }
        }

        [HttpPatch("editProfile")]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> EditProfile([FromBody] EditUserDto editModel)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            string? role = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return Unauthorized();
            if (role == "admin")
            {
                if (!string.IsNullOrEmpty(editModel.Phone) && await _rep.GetUserByPhone(editModel.Phone) != null)
                {
                    return Conflict(new { errorText = "Пользователь с таким телефоном уже существует" });
                }
                else if (await _rep.EditUser(editModel))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, new { errorText = "Не удалось изменить профиль пользователя" });
                }
            }
            else
            {
                string? phoneUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
                if (string.IsNullOrEmpty(phoneUser)) return Unauthorized();
                if (editModel.OldPhone == phoneUser)
                {
                    if (!string.IsNullOrEmpty(editModel.Phone) && await _rep.GetUserByPhone(editModel.Phone) != null)
                    {
                        return Conflict(new { errorText = "Пользователь с таким телефоном уже существует" });
                    }
                    else if (await _rep.EditUser(editModel))
                    {
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(500, new { errorText = "Не удалось изменить пользователя" });
                    }
                }
                else
                {
                    return Forbid();
                }
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            var identity = await GetIdentityAsync(model.Phone, model.Password);
            if (identity == null)
            {
                return Unauthorized(new { errorText = "Неверный логин или пароль." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(_jwtOptions.LifetimeMinutes)),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new AuthResponseDto
            {
                AccessToken = encodedJwt,
                Phone = identity.FindFirst(ClaimTypes.MobilePhone)?.Value ?? string.Empty
            };
            _logger.LogInformation($@"Пользователь {model.Phone} залогинился");
            return Ok(response);
        }
        private async Task<ClaimsIdentity?> GetIdentityAsync(string phone, string password)
        {
            var person = await _rep.GetUserByPhone(phone);
            if (person != null && _passwordHasher.Verify(password, person.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.MobilePhone, person.Phone ?? string.Empty),
                    new Claim(ClaimTypes.Role, person.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            // если пользователя не найдено
            return null;
        }
    }
}
