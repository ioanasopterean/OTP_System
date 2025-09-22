using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpSecureApplication.Services;

namespace OtpSecureApplication.Pages
{
    public class OtpModel : PageModel
    {
        public readonly IOtpService _otpService;
        public string ToastMessage { get; set; }
        public string ErrorMessage { get; set; }
        public int ToastDuration { get; set; } = 9000;

        [BindProperty]
        public string UserOtp { get; set; }

        private const string UserId = "demo-user";

        public OtpModel(IOtpService otpService)
        {
            _otpService = otpService;
        }

        public void OnGet()
        {
            var otp = _otpService.GenerateOtp(UserId);
            var encrypted = _otpService.GetEncryptedOtp(UserId);
            var expiry = _otpService.GetExpiry(UserId);
            ToastMessage = $"Your OTP is: {otp} (valid {(_otpService.GetExpiry(UserId) - DateTime.UtcNow).Seconds} sec)";
            ToastDuration = (_otpService.GetExpiry(UserId) - DateTime.UtcNow).Milliseconds + 9000;
        }

        public void OnPost()
        {
            if (_otpService.IsOtpValid(UserId, UserOtp))
            {
                ToastMessage = "✅ OTP verified successfully!";
            }
            else if (DateTime.UtcNow > _otpService.GetExpiry(UserId))
            {
                ErrorMessage = "❌ OTP has expired. Please request a new one.";
            }
            else if (_otpService.GetRemainingAttempts(UserId) <= 0)
            {
                ErrorMessage = "❌ Maximum attempts exceeded.";
            }
            else
            {
                ErrorMessage = $"❌ Invalid OTP. Remaining attempts: {_otpService.GetRemainingAttempts(UserId)}";
            }
        }
    }
}