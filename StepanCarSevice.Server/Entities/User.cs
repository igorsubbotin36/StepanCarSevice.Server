using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StepanCarSevice.Server.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string Email { get; set; }
        public string Password { get; set;}
        public string? Phone { get; set; }
        public string Role { get; set; }
    }
}
