using Microsoft.EntityFrameworkCore;

namespace BookStore.Models
{
    [PrimaryKey(nameof(OrderId), nameof(BookId))]
    public class OrderItem
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        public decimal TotalPrice { get; set; }
    }
}
