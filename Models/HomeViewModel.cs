using System.Collections.Generic;

namespace HuflitRestaurant.Models
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> BestSellers { get; set; }
    }
}