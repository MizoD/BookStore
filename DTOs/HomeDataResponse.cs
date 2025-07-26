namespace BookStore.DTOs
{
    public class HomeDataResponse
    {
        public ICollection<Book> RecommendedBooks { get; set; } = new List<Book>();
        public ICollection<Book> FlashSaleBooks { get; set; } = new List<Book>();
        public ICollection<Book> BestSellerBooks { get; set; } = new List<Book>();


    }
}
