namespace OtpSecureApplication.Services
{
    public class OtpSettings
    {
        public string Key { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }
        public int MaxAttempts { get; set; }
    }
}
