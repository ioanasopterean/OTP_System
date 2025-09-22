using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace OtpSecureApplication.Services
{
    public class OtpService : IOtpService
    {
        private readonly OtpSettings _settings;
        private readonly Dictionary<string, (string otp, DateTime expiry, int attempts)> _store = new();
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public OtpService(IOptions<OtpSettings> options)
        {
            _settings = options.Value;
            _key = Encoding.UTF8.GetBytes(_settings.Key);
            _iv = Encoding.UTF8.GetBytes(_settings.IV);
        }

        public string GenerateOtp(string userId)
        {
            var otp = GenerateRandomOtp(6);
            var expiry = DateTime.UtcNow.AddSeconds(_settings.ExpirySeconds);
            _store[userId] = (otp, expiry, 0);
            return otp;
        }

        public bool IsOtpValid(string userId, string input)
        {
            if (!_store.ContainsKey(userId)) return false;
            var (otp, expiry, attempts) = _store[userId];
            if (DateTime.UtcNow > expiry || attempts >= _settings.MaxAttempts)
                return false;
            _store[userId] = (otp, expiry, attempts + 1);
            return input == otp;
        }

        public string GetEncryptedOtp(string userId)
        {
            if (!_store.ContainsKey(userId)) return "";
            var otp = _store[userId].otp;
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(otp);
            return Convert.ToBase64String(ms.ToArray());
        }

        public DateTime GetExpiry(string userId) => _store.ContainsKey(userId) ? _store[userId].expiry : DateTime.MinValue;
        public int GetRemainingAttempts(string userId) => _store.ContainsKey(userId) ? _settings.MaxAttempts - _store[userId].attempts : _settings.MaxAttempts;

        private string GenerateRandomOtp(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            var chars = bytes.Select(b => (char)('0' + (b % 10))).ToArray();
            return new string(chars);
        }
    }
}