using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class Address
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required, StringLength(100)]
        public string TinhThanh { get; set; }

        [Required, StringLength(100)]
        public string QuanHuyen { get; set; }

        [Required, StringLength(100)]
        public string PhuongXa { get; set; }

        [Required, StringLength(200)]
        public string DiaChiCuThe { get; set; }

        public bool MacDinh { get; set; }
    }
}