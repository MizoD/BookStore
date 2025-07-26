using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class CheckoutsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CheckoutsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("Success")]
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await unitOfWork.OrderRepository.GetOneAsync(e => e.Id == orderId);

            if (order is null)
                return NotFound();

            order.OrderStatus = OrderStatus.processing;

            var service = new SessionService();
            var session = service.Get(order.SessionId);

            order.PaymentId = session.PaymentIntentId;

            await unitOfWork.OrderRepository.CommitAsync();

            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }

            if (user is null)
                return NotFound();

            var carts = await unitOfWork.CartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: e=> e.Include(c=> c.Book));

            List<OrderItem> orderItems = new();
            foreach (var item in carts)
            {
                orderItems.Add(new()
                {
                    BookId = item.BookId,
                    OrderId = orderId,
                    TotalPrice = item.Book.Price * item.Count
                });
                var book = await unitOfWork.BookRepository.GetOneAsync(b=> b.Id == item.BookId);
                if (book is not null) book.Quantity -= item.Count;

            }

            await unitOfWork.OrderItemRepository.CreateRangeAsync(orderItems);

            foreach (var item in carts)
            {
                await unitOfWork.CartRepository.DeleteAsync(item);
            }

            await unitOfWork.CommitAsync();

            return Ok("Added order successfully");
        }
        [HttpGet("Cancel")]
        public IActionResult Cancel()
        {
            return NoContent();
        }
    }
}
