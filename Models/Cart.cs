using Microsoft.EntityFrameworkCore;

namespace BookStore.Models
{
    [PrimaryKey(nameof(ApplicationUserId), nameof(BookId))]
    public class Cart
    {
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int Count { get; set; }
    }
}
