using Microsoft.AspNetCore.Authorization;
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
    public class CartsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CartsController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
           
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int Id, int count)
        {
            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }

            if (user is not null)
            {
                var book = await unitOfWork.BookRepository.GetOneAsync(b=> b.Id == Id);
                if (book is null) return NotFound();

                if (book.Quantity >= count && count > 0)
                {
                    var bookInCart = await unitOfWork.CartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == Id);

                    if (bookInCart is not null)
                    {
                        bookInCart.Count += count;
                    }
                    else
                    {
                        await unitOfWork.CartRepository.CreateAsync(new()
                        {
                            ApplicationUserId = user.Id,
                            BookId = Id,
                            Count = count
                        });
                    }

                    await unitOfWork.CommitAsync();

                    return Ok("Add to Cart successfully");
                }

                return BadRequest("over the limited count");
            }

            return NotFound();
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }

            if (user is not null)
            {
                var carts = await unitOfWork.CartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: e=> e.Include(b=> b.Book));

                var totalPrice = carts.Sum(e => e.Book.Price * e.Count);

                return Ok(new
                {
                    carts,
                    totalPrice
                });
            }

            return NotFound();
        }

        [HttpPatch("IncrementCount")]
        public async Task<IActionResult> IncrementCount(int Id)
        {
            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }

            if (user is not null)
            {
                var bookInCart = await unitOfWork.CartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == Id);
                if (bookInCart is not null)
                {
                    bookInCart.Count++;
                    await unitOfWork.CommitAsync();

                    return NoContent();
                }
                return NotFound();
            }
            return NotFound();

        }

        [HttpPatch("DecrementCount")]
        public async Task<IActionResult> DecrementCount(int Id)
        {
            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }
            if (user is not null)
            {
                var bookInCart = await unitOfWork.CartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == Id);
                if (bookInCart is not null)
                {
                    if (bookInCart.Count > 1)
                    {
                        bookInCart.Count--;
                        await unitOfWork.CommitAsync();
                    }

                    return NoContent();
                }
                return NotFound();
            }
            return NotFound();

        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }
            if (user is not null)
            {
                var bookInCart = await unitOfWork.CartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.BookId == Id);
                if (bookInCart is not null)
                {
                    await unitOfWork.CartRepository.DeleteAsync(bookInCart);

                    return NoContent();
                }
                return NotFound();
            }
            return NotFound();

        }

        [HttpPost("Pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await unitOfWork.UserManager.GetUserAsync(User);
            if (user is null)
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                user = await unitOfWork.UserManager.FindByIdAsync(userId);
            }
            if (user is not null)
            {
                var carts = await unitOfWork.CartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: e=> e.Include(e=> e.Book));

                if (carts is not null)
                {

                    await unitOfWork.OrderRepository.CreateAsync(new()
                    {
                        ApplicationUserId = user.Id,
                        Date = DateTime.UtcNow,
                        OrderStatus = OrderStatus.pending,
                        PaymentMethod = PaymentMethod.Visa,
                        TotalPrice = carts.Sum(e => e.Book.Price * e.Count)
                    });

                    var order = (await unitOfWork.OrderRepository.GetAsync(e => e.ApplicationUserId == user.Id)).OrderBy(e => e.Id).LastOrDefault();

                    if (order is null)
                        return BadRequest();

                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                        SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={order.Id}",
                        CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel",
                    };


                    foreach (var item in carts)
                    {
                        options.LineItems.Add(new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "egp",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Book.Title,
                                    Description = item.Book.Description,
                                },
                                UnitAmount = (long)item.Book.Price * 100,
                            },
                            Quantity = item.Count,
                        });
                    }


                    var service = new SessionService();
                    var session = service.Create(options);
                    order.SessionId = session.Id;
                    await unitOfWork.OrderRepository.CommitAsync();
                    return Ok(new
                    {
                        url = session.Url
                    });

                }
                return NotFound();
            }
            return NotFound();


        }
    }
}
