using System;
using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        // COD, MoMo, ZaloPay, ChuyenKhoan
        [Required, StringLength(30)]
        public string PhuongThuc { get; set; }

        public decimal SoTien { get; set; }

        // ChoThanhToan, ThanhCong, ThatBai
        [Required, StringLength(30)]
        public string TrangThai { get; set; }

        [StringLength(100)]
        public string MaGiaoDich { get; set; }

        public DateTime NgayTao { get; set; }
    }
}