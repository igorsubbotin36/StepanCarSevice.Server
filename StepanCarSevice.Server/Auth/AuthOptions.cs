using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace StepanCarService.Server.Auth
{
    public class AuthOptions
    {
        public const string ISSUER = "Stepan_Korablev"; // издатель токена
        public const string AUDIENCE = "StepanCarService"; // потребитель токена
        const string KEY = "U1CufoGHEBaWyu5U1CufoGHEBaWyu5U1CufoGHEBaWyu5";   // ключ для шифрации
        public const int LIFETIME = 100; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
