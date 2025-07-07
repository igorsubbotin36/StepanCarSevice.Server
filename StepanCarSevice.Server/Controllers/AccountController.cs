using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StepanCarSevice.Server.Auth;
using StepanCarSevice.Server.DbContexts;
using StepanCarSevice.Server.Entities;
using StepanCarSevice.Server.Models;
using StepanCarSevice.Server.Repository.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace StepanCarSevice.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly PostgreDbContext _dbContext;
        private readonly IUserRepository _rep;
        public AccountController(IUserRepository rep, ILogger<AccountController> logger, PostgreDbContext db)
        {
            _logger = logger;
            _dbContext = db;
            _rep = rep;
        }
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (await _rep.GetUserByPhone(model.Phone) != null)
            {
                return BadRequest(new { errorText = "User with this email alrady exists" });
            }
            else if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { errorText = "Passwords don't match" });
            }
            if (await _rep.AddUser(model))
            {
                return Ok();
            }
            else
            {
                return BadRequest(new { errorText = "Something wrong" });
            }
        }
        [HttpDelete("/deleteUser")]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> DeleteUser(string phone)
        {
            string role = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            if (role == "admin")
            {
                if (await _rep.DeleteUser(phone))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(new { errorText = "Unable to delete user" });
                }
            }
            else
            {
                string phoneUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone).Value;
                if (phone == phoneUser)
                {
                    if (await _rep.DeleteUser(phone))
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(new { errorText = "Unable to delete user" });
                    }
                }
                else
                {
                    return BadRequest(new { errorText = "You haven't access to delete this user" });
                }
            }
        }

        [HttpGet("/getAllUsers")]
        [Authorize(Roles = "admin")]
        public async Task<List<User>> GetAllUsers()
        {
            return await _rep.GetAllUsers();
        }

        [HttpPatch("/changePassword")]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel passwordModel)
        {
            string phoneUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone).Value;
            if (passwordModel.OldPassword != null 
                && passwordModel.NewPassword != null
                && passwordModel.ConfirmPassword != null
                && passwordModel.NewPassword == passwordModel.ConfirmPassword
                && _rep.GetUserByPhone(phoneUser).Result.Password == passwordModel.OldPassword)
            {
                if (await _rep.ChangePassword(passwordModel.NewPassword, phoneUser))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(new { errorText = "Can't change password" });
                }
            }
            else
            {
                return BadRequest(new { errorText = "Passwords don't match" });
            }
        }

        [HttpPatch("/editProfile")]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> EditProfile(EditUserModel editModel)
        {
            string role = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            if (role == "admin")
            {
                if (await _rep.GetUserByPhone(editModel.Phone) != null)
                {
                    return BadRequest(new { errorText = "User with this phone already exists" });
                }
                else if (await _rep.EditUser(editModel))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(new { errorText = "Unable to edit user profile" });
                }
            }
            else
            {
                string phoneUser = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone).Value;
                if (editModel.OldPhone == phoneUser)
                {
                    if (await _rep.GetUserByPhone(editModel.Phone) != null)
                    {
                        return BadRequest(new { errorText = "User with this phone already exists" });
                    }
                    else if (await _rep.EditUser(editModel))
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(new { errorText = "Unable to edit user" });
                    }
                }
                else
                {
                    return BadRequest(new { errorText = "You haven't access to edit this user" });
                }
            }
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginModel model)
        {
            var identity = GetIdentity(model.Phone, model.Password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                email = identity.Name
            };
            _logger.LogInformation($@"Пользователь {model.Phone} залогинился");
            return Json(response);
        }
        private ClaimsIdentity GetIdentity(string phone, string password)
        {
            User person = _dbContext.Users.FirstOrDefault(x => x.Phone == phone && x.Password == password);
            if (person != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.MobilePhone, person.Phone),
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
