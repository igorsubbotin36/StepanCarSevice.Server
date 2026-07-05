using StepanCarSevice.AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Application.Models.Dto
{
    public record UserDto(List<ClaimDto> Claims);
    public record ClaimDto(string Type, string Value);
    public record UserInfoDto(string FirstName, string SecondName, string Email, string? Phone, string Role);
}
