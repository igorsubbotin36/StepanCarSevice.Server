using StepanCarSevice.AuthService.Application.Models.Dto;
using StepanCarSevice.AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface ITokenGeneratorService
    {
        AuthResponseDto GenerateToken(ClaimsIdentity identity);

        // Генерация refresh токена
        string GenerateRefreshToken();

        // Валидация и получение данных из токена
        List<Claim> ValidateToken(string token);
    }
}
