using Core.Entity;
using Core.Repository;

namespace Infrastructure.Repository
{
    public class PromotionRepository : EFRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}