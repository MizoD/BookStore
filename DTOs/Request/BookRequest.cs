using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BookStore.DTOs.Request
{
    public class BookRequest
    {
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        [Range(0, 500000)]
        public decimal Price { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public bool IsInFlashSale { get; set; }
        [Required]
        [Range(0, 50000)]
        public int Quantity { get; set; }
        public bool Status { get; set; }
        [Range(0, 100)]
        public Decimal Discount { get; set; }
        public int Traffic { get; set; }

        [Required]
        public int AuthorId { get; set; }
        [Required]
        public int PublisherId { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }
}
