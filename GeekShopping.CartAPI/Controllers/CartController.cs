using GeekShopping.CartAPI.Data.DTOs;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using GeekShopping.CouponAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CartController : ControllerBase
    {
        private ICartRepository _cartRepository;
        private ICouponRepository _couponRepository;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        public CartController(ICartRepository cartRepository, ICouponRepository couponRepository, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _cartRepository = cartRepository;
            _couponRepository = couponRepository;
            _rabbitMQMessageSender = rabbitMQMessageSender;
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartDTO>> FindById(string id)
        {
            var cart = await _cartRepository.FindCartByUserId(id);
            if (cart == null)
                return NotFound();
            return Ok(cart);

        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartDTO>> AddCart(CartDTO dto)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(dto);
            if (cart == null)
                return NotFound();
            return Ok(cart);

        }

        [HttpPut("update-cart")]
        public async Task<ActionResult<CartDTO>> UpdateCart(CartDTO dto)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(dto);
            if (cart == null)
                return NotFound();
            return Ok(cart);

        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<CartDTO>> RemoveCart(int id)
        {
            var status = await _cartRepository.RemoveFromCart(id);
            if (!status)
                return BadRequest();
            return Ok(status);

        }

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<CartDTO>> ApplyCoupon(CartDTO dto)
        {
            var status = await _cartRepository.ApplyCoupon(dto.CartHeader.UserID, dto.CartHeader.CouponCode);
            if (status)
                return Ok(status);
            return NotFound();
            

        }

        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartDTO>> RemoveCoupon(string userId)
        {
            var status = await _cartRepository.RemoveCoupon(userId);
            if (status)
                return Ok(status);
            return NotFound();
            

        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderDTO>> Checkout(CheckoutHeaderDTO dto)
        {
            string token = Request.Headers["Authorization"];
            if (dto?.UserID == null)
            {
                return BadRequest();
            }
            var cart = await _cartRepository.FindCartByUserId(dto.UserID);
            if (cart == null)  return NotFound();
            if (!string.IsNullOrEmpty(dto.CouponCode))
            {
                CouponDTO coupon = await _couponRepository.GetCoupon(dto.CouponCode, token);
                if (dto.DiscountAmount != coupon.DiscountAmount)
                {
                    return StatusCode(412);
                }
                
            }

            dto.CartDetails = cart.CartDetails;
            dto.DateTime = DateTime.Now;


            //RabbitMQ logic comes here
            _rabbitMQMessageSender.Send(dto,"chekoutqueue");

            await _cartRepository.ClearCart(dto.UserID);
            return Ok(dto);
            

        }
    }
}
