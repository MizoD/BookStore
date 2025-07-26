using System.ComponentModel.DataAnnotations;

namespace BookStore.DTOs.Request
{
    public class AuthorRequest
    {
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        public string? Bio { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }
    }
}
