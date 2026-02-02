using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Application.Models.Dto
{
    public class UserDto
    {
        public List<ClaimDto> Claims { get; set; }
    }
    public class ClaimDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
