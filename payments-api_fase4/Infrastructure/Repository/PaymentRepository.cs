using Core.Entity;
using Core.Repository;

namespace Infrastructure.Repository;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Payment Add(Payment payment)
    {
        _context.Payments.Add(payment);
        _context.SaveChanges();
        return payment;
    }

    public Payment Update(Payment payment)
    {
        _context.Payments.Update(payment);
        _context.SaveChanges();
        return payment;
    }

    public IEnumerable<Payment> GetAll()
    {
        return _context.Payments
            .OrderByDescending(x => x.Id)
            .ToList();
    }
}