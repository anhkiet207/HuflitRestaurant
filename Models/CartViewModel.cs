using System.Collections.Generic;
using System.Linq;

namespace HuflitRestaurant.Models
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TongTien => Items.Sum(i => i.ThanhTien);
    }
}