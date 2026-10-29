using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Ten { get; set; }

        [StringLength(500)]
        public string MoTa { get; set; }

        public decimal Gia { get; set; }

        [StringLength(300)]
        public string HinhAnh { get; set; }

        public bool BanChay { get; set; }

        public bool DangBan { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

    }

}