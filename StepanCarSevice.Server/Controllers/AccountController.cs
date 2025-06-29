using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StepanCarSevice.Server.DbContexts;
using StepanCarSevice.Server.Entities;
using StepanCarSevice.Server.Auth;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StepanCarSevice.Server.Models;

namespace StepanCarSevice.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly PostgreDbContext _dbContext;
        public AccountController(ILogger<AccountController> logger, PostgreDbContext db)
        {
            _logger = logger;
            _dbContext = db;
        }
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            User person = _dbContext.Users.FirstOrDefault(x => x.Email == model.Email);
            if (person != null)
            {
                return BadRequest(new { errorText = "User with this email alrady exists" });
            }
            else if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { errorText = "Passwords don't match" });
            }
            try
            {
                User newUser = new User() { Email = model.Email, Password = model.Password, Role = "user" };
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                return Ok(); //// перенести в crud
            }
            catch
            {
                return BadRequest(new { errorText = "Something wrong" });
            }
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginModel model)
        {
            var identity = GetIdentity(model.Email, model.Password);
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

            return Json(response);
        }
        private ClaimsIdentity GetIdentity(string email, string password)
        {
            User person = _dbContext.Users.FirstOrDefault(x => x.Email == email && x.Password == password);
            if (person != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, person.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            // если пользователя не найдено
            return null;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
