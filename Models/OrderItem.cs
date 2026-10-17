using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        // Lưu lại tên và giá tại thời điểm đặt (sau này đổi giá món cũng không ảnh hưởng đơn cũ)
        [Required, StringLength(150)]
        public string TenMon { get; set; }

        public decimal DonGia { get; set; }

        public int SoLuong { get; set; }
    }
}