namespace OtpSecureApplication.Services
{
    public interface IOtpService
    {
        string GenerateOtp(string userId);
        bool IsOtpValid(string userId, string input);
        string GetEncryptedOtp(string userId);
        DateTime GetExpiry(string userId);
        int GetRemainingAttempts(string userId);
    }
}