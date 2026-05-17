using Core.Entity;
using Core.Input;

namespace Core.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(PaymentInput input);
        IEnumerable<Payment> GetAll();
        Payment? GetById(int id);
        Payment? GetByPurchaseId(int purchaseId);
    }
}