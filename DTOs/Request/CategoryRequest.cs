using System.ComponentModel.DataAnnotations;

namespace BookStore.DTOs.Request
{
    public class CategoryRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = null!;
        public bool Status { get; set; }
    }
}
