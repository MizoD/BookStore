using System.ComponentModel.DataAnnotations;

namespace BookStore.DTOs.Request
{
    public class EditProfileRequest
    {
        
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;
        
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;
        
        public string? Address { get; set; }
        public IFormFile? ImageUrl { get; set; } 
    }
}
