using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = $"{SD.SuperAdmin}, {SD.Admin}")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await unitOfWork.CategoryRepository.GetAsync();

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CategoryRequest categoryRequest)
        {
            await unitOfWork.CategoryRepository.CreateAsync(categoryRequest.Adapt<Category>());

            var lastCategory = (await unitOfWork.CategoryRepository.GetAsync()).OrderBy(e => e.Id).LastOrDefault();

            if (lastCategory is null)
                return NotFound();


            return CreatedAtAction(nameof(GetOne), "Categories", new { id = lastCategory.Id });
        }

        [HttpGet("GetOne/{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var category = await unitOfWork.CategoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {
                return Ok(category);
            }

            return NotFound();
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, CategoryRequest categoryRequest)
        {
            var category = await unitOfWork.CategoryRepository.GetOneAsync(e => e.Id == id);
            if (category is null) return NotFound();

            category.Name = categoryRequest.Name;
            category.Description = categoryRequest.Description;

            await unitOfWork.CategoryRepository.CommitAsync();

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await unitOfWork.CategoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {
                await unitOfWork.CategoryRepository.DeleteAsync(category);

                return NoContent();
            }

            return NotFound();
        }
    }
}
