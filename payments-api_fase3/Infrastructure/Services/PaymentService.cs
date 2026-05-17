using Core.Entity;
using Core.Events;
using Core.Input;
using Core.Repository;
using Core.Services;
using Infrastructure.CrossCutting.Correlation;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(
            IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public IEnumerable<Payment> GetAll()
        {
            return _paymentRepository.GetAll();
        }

        public Payment? GetById(int id)
        {
            return _paymentRepository.GetAll().FirstOrDefault(x => x.Id == id);
        }

        public Payment? GetByPurchaseId(int purchaseId)
        {
            return _paymentRepository.GetAll().FirstOrDefault(x => x.PurchaseId == purchaseId);
        }

        public Task<Payment> ProcessPaymentAsync(PaymentInput input)
        {
            ValidarInput(input);

            var payment = new Payment
            {
                PurchaseId = input.PurchaseId,
                UserId = input.UserId,
                Amount = input.Amount,
                GameId = input.GameId,
                Status = "Processing"
            };

            var approved = SimularResultadoPagamento();

            payment.Status = approved ? "Approved" : "Rejected";

            var createdPayment = _paymentRepository.Add(payment);

            return Task.FromResult(createdPayment);
        }

        private static void ValidarInput(PaymentInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.PurchaseId <= 0)
                throw new Exception("PurchaseId inválido.");

            if (input.UserId <= 0)
                throw new Exception("UserId inválido.");

            if (input.GameId <= 0)
                throw new Exception("GameId inválido.");

            if (input.Amount <= 0)
                throw new Exception("Amount deve ser maior que zero.");
        }

        private static bool SimularResultadoPagamento()
        {
            return Random.Shared.Next(1, 101) > 20;
        }
    }
}