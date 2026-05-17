using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Input
{
    public class PromotionUpdateInput
    {
        public required int Id { get; set; }
        public required int GameId { get; set; }
        public required decimal DiscountPercentage { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required bool IsActive { get; set; }
    }
}
