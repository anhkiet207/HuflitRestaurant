using System.Linq;
using System.Web.Mvc;
using HuflitRestaurant.Models;
namespace HuflitRestaurant.Controllers
{
    public class ProductController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Product
        public ActionResult Index(int? categoryId, string keyword)
        {
            var query = db.Products.Where(p => p.DangBan);

            // Lọc theo danh mục nếu có
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Tìm kiếm theo tên món nếu có
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Ten.Contains(keyword));
            }

            var vm = new ProductViewModel
            {
                Categories = db.Categories.OrderBy(c => c.Id).ToList(),
                Products = query.OrderBy(p => p.Id).ToList(),
                SelectedCategoryId = categoryId,
                SearchKeyword = keyword
            };

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}