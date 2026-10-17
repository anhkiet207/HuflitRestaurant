using System.Collections.Generic;

namespace HuflitRestaurant.Models
{
    public class ProductViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SearchKeyword { get; set; }
    }
}