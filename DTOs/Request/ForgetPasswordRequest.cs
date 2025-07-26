using System.ComponentModel.DataAnnotations;

namespace BookStore.DTOs.Request
{
    public class ForgetPasswordRequest
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;
    }
}
