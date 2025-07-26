
namespace BookStore.Repositories
{
    public class PublisherRepository : Repository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
