using Core.Entity;
using Core.Input;

namespace Core.Services
{
    public interface IPurchaseService : IBaseService<Purchase>
    {
        Task<Purchase> CreatePurchaseAsync(PurchaseInput input);
        Task ProcessPaymentResultAsync(int purchaseId, string status, DateTime? processedAt = null);

        IList<PurchaseDto> ObterTodosDto();
        PurchaseDto? ObterPorIdDto(int id);
        IList<PurchaseDto> ObterPorUserDto(int userId);
        IList<UserLibraryDto> GetLibraryByUserDto(int userId);
    }
}