using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Publisher
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public string? WebsiteUrl { get; set; }
        [Required]
        public string Address { get; set; } = null!;

        public DateTime EstablishedDate { get; set; }

        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public string Country { get; set; } = null!;

        public string CEOName { get; set; } = null!;


    }
}
