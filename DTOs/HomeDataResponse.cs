namespace BookStore.DTOs
{
    public class HomeDataResponse
    {
        public List<Book> RecommendedBooks { get; set; } = new();
        public List<Book> FlashSaleBooks { get; set; } = new();
        public List<Book> BestSellerBooks { get; set; } = new();
        

    }
}
