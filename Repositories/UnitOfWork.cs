using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;

        public UnitOfWork(IApplicationUserOTPRepository applicationUserOTPRepository, IAuthorRepository authorRepository
            , IBookRepository bookRepository, ICartRepository cartRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository,
            IPublisherRepository publisherRepository, ICategoryRepository categoryRepository,ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            ApplicationUserOTPRepository = applicationUserOTPRepository;
            AuthorRepository = authorRepository;
            BookRepository = bookRepository;
            CartRepository = cartRepository;
            OrderRepository = orderRepository;
            OrderItemRepository = orderItemRepository;
            PublisherRepository = publisherRepository;
            CategoryRepository = categoryRepository;
            this.dbContext = dbContext;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public IApplicationUserOTPRepository ApplicationUserOTPRepository { get; }
        public IAuthorRepository AuthorRepository { get; }
        public IBookRepository BookRepository { get; }
        public ICartRepository CartRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IOrderItemRepository OrderItemRepository { get; }
        public IPublisherRepository PublisherRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }

        public async Task<bool> CommitAsync()
        {
            try
            {
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
                return false;
            }
        }
        public void Dispose()
        {
            dbContext.Dispose();
        }

    }
}
