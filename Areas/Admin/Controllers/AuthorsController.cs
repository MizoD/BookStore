using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = $"{SD.SuperAdmin}, {SD.Admin}")]
    public class AuthorsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public AuthorsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await unitOfWork.AuthorRepository.GetAsync();

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] AuthorRequest authorRequest)
        {
            await unitOfWork.AuthorRepository.CreateAsync(authorRequest.Adapt<Author>());

            var lastAuthor = (await unitOfWork.AuthorRepository.GetAsync()).OrderBy(e => e.Id).LastOrDefault();

            if (lastAuthor is null)
                return NotFound();


            return CreatedAtAction(nameof(GetOne), "Authors", new { id = lastAuthor.Id });
        }

        [HttpGet("GetOne/{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var author = await unitOfWork.AuthorRepository.GetOneAsync(e => e.Id == id);

            if (author is not null)
            {
                return Ok(author);
            }

            return NotFound();
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, AuthorRequest authorRequest)
        {
            var author = await unitOfWork.AuthorRepository.GetOneAsync(e => e.Id == id);
            if (author is null) return NotFound();

            author.FullName = authorRequest.FullName;
            author.Email = authorRequest.Email;
            author.PhoneNumber = authorRequest.PhoneNumber;
            author.ProfilePictureUrl = authorRequest.ProfilePictureUrl;
            author.DateOfBirth = authorRequest.DateOfBirth;
            author.Bio = authorRequest.Bio;

            await unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var author = await unitOfWork.AuthorRepository.GetOneAsync(e => e.Id == id);

            if (author is not null)
            {
                await unitOfWork.AuthorRepository.DeleteAsync(author);

                return NoContent();
            }

            return NotFound();
        }
    }
}
