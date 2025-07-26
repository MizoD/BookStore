namespace BookStore.Models
{
    public enum OrderStatus
    {
        pending,
        processing,
        shipped,
        inWay,
        Completed,
        Canceled,
        Refunded
    }

    public enum PaymentMethod
    {
        Visa,
        Cash
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime ShippedDate { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public string? Carrier { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentId { get; set; }
        public string? SessionId { get; set; }

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new();

    }
}
