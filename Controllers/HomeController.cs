using System.Linq;
using System.Web.Mvc;
using HuflitRestaurant.Models;
namespace HuflitRestaurant.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var vm = new HomeViewModel
            {
                Categories = db.Categories.OrderBy(c => c.Id).ToList(),
                BestSellers = db.Products
                                .Where(p => p.BanChay && p.DangBan)
                                .OrderBy(p => p.Id)
                                .Take(4)
                                .ToList()
            };
            return View(vm);
        }

        public ActionResult About()
        {
            return View();
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