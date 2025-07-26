using System.ComponentModel.DataAnnotations;

namespace BookStore.DTOs.Request
{
    public class ResetPasswordRequest
    {
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string OTP { get; set; } = null!;
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
