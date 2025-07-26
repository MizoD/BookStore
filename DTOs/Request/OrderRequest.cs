namespace BookStore.DTOs.Request
{
    public class OrderRequest
    {
        public DateTime Date { get; set; }
        public DateTime ShippedDate { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public string? Carrier { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentId { get; set; }
        public string? SessionId { get; set; }

        public string ApplicationUserId { get; set; } = null!;
    }
}
