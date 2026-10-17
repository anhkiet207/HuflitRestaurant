using System.Collections.Generic;

namespace HuflitRestaurant.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string PaymentMethod { get; set; }
        public string GhiChu { get; set; }
    }
}