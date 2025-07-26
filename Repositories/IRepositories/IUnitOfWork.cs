using Microsoft.AspNetCore.Identity;

namespace BookStore.Repositories.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IApplicationUserOTPRepository ApplicationUserOTPRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        IBookRepository BookRepository { get; }
        ICartRepository CartRepository { get; }
        IOrderRepository OrderRepository { get; }
        IOrderItemRepository OrderItemRepository { get; }
        IPublisherRepository PublisherRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        UserManager<ApplicationUser> UserManager { get; }
        SignInManager<ApplicationUser> SignInManager { get; }
        Task<bool> CommitAsync();
    }
}
