using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [Required]
        [StringLength(100)]
        public string MatKhau { get; set; }

        [StringLength(20)]
        public string VaiTro { get; set; } = "Customer";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties (nếu có)
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}