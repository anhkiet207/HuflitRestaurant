using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        public decimal TongTien { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; } = "Pending";

        [StringLength(255)]
        public string DiaChiGiao { get; set; }

        [StringLength(20)]
        public string SoDienThoaiGiao { get; set; }

        public string GhiChu { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}