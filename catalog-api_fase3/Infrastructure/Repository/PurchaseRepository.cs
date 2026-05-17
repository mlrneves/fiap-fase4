using Core.Entity;
using Core.Repository;

namespace Infrastructure.Repository
{
    public class PurchaseRepository : EFRepository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
