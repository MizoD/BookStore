using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = $"{SD.SuperAdmin}, {SD.Admin}")]
    public class BooksController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public BooksController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await unitOfWork.BookRepository.GetAsync();

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] BookRequest bookRequest)
        {
            await unitOfWork.BookRepository.CreateAsync(bookRequest.Adapt<Book>());

            var lastBook = (await unitOfWork.BookRepository.GetAsync()).OrderBy(e => e.Id).LastOrDefault();

            if (lastBook is null)
                return NotFound();


            return CreatedAtAction(nameof(GetOne), "Books", new { id = lastBook.Id });
        }

        [HttpGet("GetOne/{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var book = await unitOfWork.BookRepository.GetOneAsync(e => e.Id == id);

            if (book is not null)
            {
                return Ok(book);
            }

            return NotFound();
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, BookRequest bookRequest)
        {
            var book = await unitOfWork.BookRepository.GetOneAsync(e => e.Id == id);
            if (book is null) return NotFound();

            book.Title = bookRequest.Title;
            book.Description = bookRequest.Description;
            book.Discount = bookRequest.Discount;
            book.ImageUrl = bookRequest.ImageUrl;
            book.IsRecommended = bookRequest.IsRecommended;
            book.Price = bookRequest.Price;
            book.IsInFlashSale = bookRequest.IsInFlashSale;
            book.AuthorId = bookRequest.AuthorId;
            book.CategoryId = bookRequest.CategoryId;
            book.PublisherId = bookRequest.PublisherId;
            book.Quantity = bookRequest.Quantity;
            book.Status = bookRequest.Status;

            await unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await unitOfWork.BookRepository.GetOneAsync(e => e.Id == id);

            if (book is not null)
            {
                await unitOfWork.BookRepository.DeleteAsync(book);

                return NoContent();
            }

            return NotFound();
        }
    }
}
