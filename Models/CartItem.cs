namespace HuflitRestaurant.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Ten { get; set; }
        public string HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        public decimal ThanhTien => Gia * SoLuong;
    }
}