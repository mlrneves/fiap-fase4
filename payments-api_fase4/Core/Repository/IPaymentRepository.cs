using Core.Entity;

namespace Core.Repository
{
    public interface IPaymentRepository
    {
        Payment Add(Payment payment);
        Payment Update(Payment payment);
        IEnumerable<Payment> GetAll();
    }
}