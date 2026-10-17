using System.Data.Entity;

namespace HuflitRestaurant.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection")
        {
            // Tắt kiểm tra sự khác biệt giữa Model và Database
            Database.SetInitializer<AppDbContext>(null);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}