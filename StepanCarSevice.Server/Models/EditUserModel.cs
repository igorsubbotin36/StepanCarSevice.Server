namespace StepanCarSevice.Server.Models
{
    public class EditUserModel
    {
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string OldPhone { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
