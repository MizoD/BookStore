using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Admin")]
    [ApiController]
    [Authorize(Roles = $"{SD.SuperAdmin}, {SD.Admin}")]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await unitOfWork.OrderRepository.GetAsync();

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] OrderRequest orderRequest)
        {
            await unitOfWork.OrderRepository.CreateAsync(orderRequest.Adapt<Order>());

            var lastOrder = (await unitOfWork.OrderRepository.GetAsync()).OrderBy(e => e.Id).LastOrDefault();

            if (lastOrder is null)
                return NotFound();


            return CreatedAtAction(nameof(GetOne), "Orders", new { id = lastOrder.Id });
        }

        [HttpGet("GetOne/{id}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var order = await unitOfWork.OrderRepository.GetOneAsync(e => e.Id == id);

            if (order is not null)
            {
                return Ok(order);
            }

            return NotFound();
        }


        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, OrderRequest orderRequest)
        {
            var order = await unitOfWork.OrderRepository.GetOneAsync(e => e.Id == id);
            if (order is null) return NotFound();

            order.TotalPrice = orderRequest.TotalPrice;
            order.Date = orderRequest.Date;
            order.ShippedDate = orderRequest.ShippedDate;
            order.Carrier = orderRequest.Carrier;
            order.OrderStatus = orderRequest.OrderStatus;
            order.PaymentMethod = orderRequest.PaymentMethod;
            order.ApplicationUserId = orderRequest.ApplicationUserId;
            order.PaymentId = orderRequest.PaymentId;
            order.SessionId = orderRequest.SessionId;

            await unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await unitOfWork.OrderRepository.GetOneAsync(e => e.Id == id);

            if (order is not null)
            {
                await unitOfWork.OrderRepository.DeleteAsync(order);

                return NoContent();
            }

            return NotFound();
        }
    }
}
