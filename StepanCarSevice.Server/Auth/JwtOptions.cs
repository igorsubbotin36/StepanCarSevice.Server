namespace StepanCarService.Server.Auth
{
    public class JwtOptions
    {
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string Key { get; set; }
        public int LifetimeMinutes { get; set; }
    }
}


