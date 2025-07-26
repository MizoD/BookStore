using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Customer")]
    [ApiController]
    public class HomeDataController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public HomeDataController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            int dFSale = 50;
            var books =  await unitOfWork.BookRepository.GetAsync();
            if (books is null) return BadRequest("There Is No Books");

            var bestSellersBooks = books.OrderBy(b=> b.Traffic).Skip(0).Take(10);
            var recommendBooks = books.Where(b=> b.IsRecommended);
            var flashSaleBooks = books.Where(b => b.Discount >= dFSale);


            return Ok(new HomeDataResponse()
            {
                BestSellerBooks = bestSellersBooks.ToList(),
                RecommendedBooks = recommendBooks.ToList(),
                FlashSaleBooks = flashSaleBooks.ToList()
            });
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search(string name)
        {
            var book = await unitOfWork.BookRepository.GetOneAsync(b=> b.Title.Contains(name));
            if (book is null) return BadRequest("There is no such a book");

            return RedirectToAction(nameof(Details), "HomeData", new { Id = book.Id });
        }

        [HttpPost("Details/{Id}")]
        public async Task<IActionResult> Details(int Id)
        {
            var book = await unitOfWork.BookRepository.GetOneAsync(b=> b.Id == Id);
            if (book is null) return BadRequest("There is a problem loading this book");

            return Ok(book);
        }
    }
}
