using Microsoft.Extensions.Options;
using OtpSecureApplication.Services;
using System;
using System.Text;
using Xunit;

namespace OtpSecureApplication.Tests
{

    public class OtpServiceTests
    {
        private OtpSettings GetTestSettings() => new OtpSettings
        {
            Key = "1234567890123456",
            IV = "6543210987654321",
            ExpirySeconds = 2,
            MaxAttempts = 3
        };

        [Fact]
        public void GenerateOtp_ShouldStoreAndReturnOtp()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user1";
            var otp = service.GenerateOtp(userId);

            Assert.False(string.IsNullOrEmpty(otp));
            Assert.Equal(6, otp.Length);
        }

        [Fact]
        public void IsOtpValid_ShouldReturnTrueForCorrectOtp()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user2";
            var otp = service.GenerateOtp(userId);

            Assert.True(service.IsOtpValid(userId, otp));
        }

        [Fact]
        public void IsOtpValid_ShouldReturnFalseForWrongOtp()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user3";
            service.GenerateOtp(userId);

            Assert.False(service.IsOtpValid(userId, "000000"));
        }

        [Fact]
        public void IsOtpValid_ShouldReturnFalseAfterExpiry()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user4";
            var otp = service.GenerateOtp(userId);

            System.Threading.Thread.Sleep(2500);
            Assert.False(service.IsOtpValid(userId, otp));
        }

        [Fact]
        public void IsOtpValid_ShouldReturnFalseAfterMaxAttempts()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user5";
            var otp = service.GenerateOtp(userId);

            for (int i = 0; i < 3; i++)
                service.IsOtpValid(userId, "badotp");

            Assert.False(service.IsOtpValid(userId, otp));
        }

        [Fact]
        public void GetEncryptedOtp_ShouldReturnDecryptableOtp()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user6";
            var otp = service.GenerateOtp(userId);
            var encrypted = service.GetEncryptedOtp(userId);

            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(GetTestSettings().Key);
            aes.IV = Encoding.UTF8.GetBytes(GetTestSettings().IV);
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var bytes = Convert.FromBase64String(encrypted);
            using var ms = new System.IO.MemoryStream(bytes);
            using var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read);
            using var sr = new System.IO.StreamReader(cs);
            var decryptedOtp = sr.ReadToEnd();

            Assert.Equal(otp, decryptedOtp);
        }

        [Fact]
        public void GetExpiry_ShouldReturnCorrectExpiry()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user7";
            service.GenerateOtp(userId);
            var expiry = service.GetExpiry(userId);

            Assert.True(expiry > DateTime.UtcNow);
        }

        [Fact]
        public void GetRemainingAttempts_ShouldDecreaseOnEachTry()
        {
            var service = new OtpService(Options.Create(GetTestSettings()));
            var userId = "user8";
            service.GenerateOtp(userId);

            Assert.Equal(3, service.GetRemainingAttempts(userId));
            service.IsOtpValid(userId, "badotp");
            Assert.Equal(2, service.GetRemainingAttempts(userId));
            service.IsOtpValid(userId, "badotp");
            Assert.Equal(1, service.GetRemainingAttempts(userId));
        }
    }
}

