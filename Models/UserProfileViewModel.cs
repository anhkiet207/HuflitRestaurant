using System.Collections.Generic;

namespace HuflitRestaurant.Models
{
    public class UserProfileViewModel
    {
        public User User { get; set; }
        public List<Order> Orders { get; set; }
    }
}