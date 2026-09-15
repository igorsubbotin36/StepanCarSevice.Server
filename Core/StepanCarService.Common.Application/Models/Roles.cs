namespace StepanCarService.Common.Application.Models
{
    public static class Roles
    {
        // Пользователи портала (без тенанта)
        public const string GodMode = "GodMode";
        public const string TenantOwner = "TenantOwner";

        // Пользователи тенанта
        public const string TenantModerator = "TenantModerator";
        public const string User = "User";
    }
}
