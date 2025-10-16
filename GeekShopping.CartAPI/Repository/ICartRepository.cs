using GeekShopping.CartAPI.Data.DTOs;

namespace GeekShopping.CartAPI.Repository
{
    public interface ICartRepository
    {
        Task<CartDTO> FindCartByUserId(string userID);
        Task<CartDTO> SaveOrUpdateCart(CartDTO cart);
        Task<bool> RemoveFromCart(long cartDetailsId);
        Task<bool> ApplyCoupon(string userID, string couponCode);
        Task<bool> RemoveCoupon(string userID);
        Task<bool> ClearCart(string userID);
    }
}
