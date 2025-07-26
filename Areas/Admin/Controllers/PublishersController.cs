using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = $"{SD.SuperAdmin}, {SD.Admin}")]
    public class PublishersController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public PublishersController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await unitOfWork.PublisherRepository.GetAsync();

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] PublisherRequest publisherRequest)
        {
            await unitOfWork.PublisherRepository.CreateAsync(publisherRequest.Adapt<Publisher>());

            var lastPublisher = (await unitOfWork.PublisherRepository.GetAsync()).OrderBy(e => e.Id).LastOrDefault();

            if (lastPublisher is null)
                return NotFound();


            return CreatedAtAction(nameof(GetOne), "Publishers", new { id = lastPublisher.Id });
        }

        [HttpGet("GetOne/{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var author = await unitOfWork.PublisherRepository.GetOneAsync(e => e.Id == id);

            if (author is not null)
            {
                return Ok(author);
            }

            return NotFound();
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, PublisherRequest publisherRequest)
        {
            var publisher = await unitOfWork.PublisherRepository.GetOneAsync(e => e.Id == id);
            if (publisher is null) return NotFound();

            publisher.Name = publisherRequest.Name;
            publisher.Email = publisherRequest.Email;
            publisher.PhoneNumber = publisherRequest.PhoneNumber;
            publisher.LogoUrl = publisherRequest.LogoUrl;
            publisher.Address = publisherRequest.Address;
            publisher.Description = publisherRequest.Description;
            publisher.EstablishedDate = publisherRequest.EstablishedDate;
            publisher.CEOName = publisherRequest.CEOName;
            publisher.Country = publisherRequest.Country;
            publisher.IsActive = publisherRequest.IsActive;
            publisher.WebsiteUrl = publisherRequest.WebsiteUrl;

            await unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var publisher = await unitOfWork.PublisherRepository.GetOneAsync(e => e.Id == id);

            if (publisher is not null)
            {
                await unitOfWork.PublisherRepository.DeleteAsync(publisher);

                return NoContent();
            }

            return NotFound();
        }
    }
}
