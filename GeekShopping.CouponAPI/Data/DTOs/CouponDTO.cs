using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekShopping.CouponAPI.Data.DTOs
{
    public class CouponDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
