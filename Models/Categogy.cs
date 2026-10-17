using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HuflitRestaurant.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Ten { get; set; }

        [StringLength(300)]
        public string MoTa { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}