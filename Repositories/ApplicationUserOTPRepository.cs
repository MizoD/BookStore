
namespace BookStore.Repositories
{
    public class ApplicationUserOTPRepository : Repository<ApplicationUserOTP>, IApplicationUserOTPRepository
    {
        public ApplicationUserOTPRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
