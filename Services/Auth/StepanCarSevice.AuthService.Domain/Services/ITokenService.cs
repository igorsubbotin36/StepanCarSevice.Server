using StepanCarSevice.AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Domain.Services
{
    public interface ITokenService
    {
        string GenerateTokenAsync(User user);
        bool ValidateTokenAsync(string token);
    }
}
