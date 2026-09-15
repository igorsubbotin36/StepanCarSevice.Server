using StepanCarSevice.AuthService.Application.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Infrastructure.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 100_000;
        // Число итераций берётся из хэша: ограничиваем, чтобы подменённый хэш не нагрузил CPU
        private const int MinIterations = 10_000;
        private const int MaxIterations = 1_000_000;
        private const char Delimiter = ':';

        public string Hash(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256);
            var key = algorithm.GetBytes(KeySize);
            var salt = algorithm.Salt;
            return string.Join(Delimiter, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        public bool Verify(string password, string hash)
        {
            // Повреждённый хэш — это «пароль не подходит», а не исключение и 500
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash)) return false;
            var parts = hash.Split(Delimiter);
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out var iterations)
                || iterations < MinIterations || iterations > MaxIterations) return false;
            byte[] salt, key;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                key = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }
            if (salt.Length != SaltSize || key.Length != KeySize) return false;

            using var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);
            var keyToCheck = algorithm.GetBytes(KeySize);
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }
    }
}
