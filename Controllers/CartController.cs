using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HuflitRestaurant.Models;

namespace HuflitRestaurant.Controllers
{
    public class CartController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // Danh sách mã giảm giá do Admin cấp
        private static Dictionary<string, decimal> adminCoupons = new Dictionary<string, decimal>
        {
            { "HUFLIT10K", 10000 },
            { "HUFLIT20K", 20000 },
            { "HUFLIT50K", 50000 },
            { "DISCOUNT30", 30000 },
            {"ANHKIETDEPTRAI",300000}
        };

        private List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        private void UpdateCartCount()
        {
            var cart = GetCart();
            Session["CartCount"] = cart.Sum(i => i.SoLuong);
        }

        public ActionResult Index()
        {
            var cart = GetCart();
            var vm = new CartViewModel { Items = cart };
            return View(vm);
        }

        [HttpPost]
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id && p.DangBan);
            if (product != null)
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(i => i.ProductId == id);
                if (item != null)
                {
                    item.SoLuong += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Ten = product.Ten,
                        HinhAnh = product.HinhAnh,
                        Gia = product.Gia,
                        SoLuong = quantity
                    });
                }
                UpdateCartCount();
            }

            return Json(new { success = true, cartCount = Session["CartCount"] });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.SoLuong = quantity;
                }
                else
                {
                    cart.Remove(item);
                }
                UpdateCartCount();
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                UpdateCartCount(); // Cập nhật lại số lượng giỏ hàng ở header
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ApplyCoupon(string couponCode)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            string code = (couponCode ?? "").Trim().ToUpper();

            if (adminCoupons.ContainsKey(code))
            {
                decimal discountAmount = adminCoupons[code];
                Session["Discount"] = discountAmount;
                Session["CouponCode"] = code;
                TempData["CouponSuccess"] = $"Đã áp dụng mã {code} (-{discountAmount:#,##0} đ)";
            }
            else
            {
                TempData["CouponError"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn!";
            }

            return RedirectToAction("Checkout");
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            var model = new CartViewModel
            {
                Items = cart
            };

            ViewBag.Discount = Session["Discount"] != null ? (decimal)Session["Discount"] : 0;
            ViewBag.CouponCode = Session["CouponCode"] != null ? Session["CouponCode"].ToString() : "";

            return View(model);
        }

        [HttpPost]
        public ActionResult ProcessCheckout(string DiaChi, string SoDienThoai, string GhiChu, string PaymentMethod)
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            // 1. Bắt buộc đăng nhập (trước đây nếu chưa đăng nhập thì đơn bị gán cho UserId = 1)
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["UserId"];

            // 2. Tính tổng tiền đã trừ giảm giá
            decimal tamTinh = cart.Sum(x => x.Gia * x.SoLuong);
            decimal discount = Session["Discount"] != null ? (decimal)Session["Discount"] : 0;
            decimal tongTien = tamTinh - discount;
            if (tongTien < 0) tongTien = 0;

            bool isBank = (PaymentMethod == "Bank");
            string ptttText = isBank ? "Chuyển khoản / Quét mã QR" : "Thanh toán khi nhận hàng (COD)";
            string safeGhiChu = string.IsNullOrEmpty(GhiChu) ? ("PTTT: " + ptttText) : (GhiChu + " | PTTT: " + ptttText);
            
            // 3. Tạo mới Đơn hàng (Order)
            var order = new Order
            {
                UserId = userId,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThai = "Pending",
                DiaChiGiao = string.IsNullOrEmpty(DiaChi) ? "Chưa nhập" : DiaChi,
                SoDienThoaiGiao = string.IsNullOrEmpty(SoDienThoai) ? "" : SoDienThoai,
                GhiChu = safeGhiChu
            };

            db.Configuration.ValidateOnSaveEnabled = false;

            db.Orders.Add(order);
            db.SaveChanges();

            // 4. Lưu từng món ăn vào Chi tiết đơn hàng (OrderItem)
            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia,
                    TenMon = item.Ten
                };
                db.OrderItems.Add(orderItem);
            }
            db.SaveChanges();

            db.Configuration.ValidateOnSaveEnabled = true;

            // 5. Xóa giỏ hàng và mã giảm giá (dùng chung cho COD và QR)
            Session["Cart"] = null;
            Session["CartCount"] = 0;
            Session["Discount"] = null;
            Session["CouponCode"] = null;

            // 6a. Chuyển khoản QR -> hiện mã QR trước, sau đó khách bấm "Tôi đã chuyển khoản"
            if (isBank)
            {
                TempData["TongTien"] = tongTien;
                TempData["SoDienThoai"] = SoDienThoai;
                TempData["OrderId"] = order.Id;
                Session["LastOrderId"] = order.Id; // để nút "Tôi đã chuyển khoản" luôn lấy được mã đơn

                return RedirectToAction("PaymentQR");
            }

            // 6b. COD -> sang thẳng trang Đặt hàng thành công
            return RedirectToAction("OrderSuccess", new { id = order.Id, pm = "COD" });
        }

        // GET: Cart/OrderSuccess/5
        // GET: Cart/OrderSuccess/5
        public ActionResult OrderSuccess(int? id, string pm)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["UserId"];

            // Lấy mã đơn: ưu tiên trên địa chỉ, nếu không có thì dùng đơn vừa đặt trong Session
            int orderId;
            if (id.HasValue)
            {
                orderId = id.Value;
            }
            else if (Session["LastOrderId"] != null)
            {
                orderId = (int)Session["LastOrderId"];
            }
            else
            {
                return RedirectToAction("Profile", "Account");
            }

            // Chỉ chủ đơn hàng mới xem được trang này
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.Items = db.OrderItems.Where(i => i.OrderId == order.Id).ToList();
            ViewBag.IsBank = (pm == "Bank");
            return View(order);
        }
        [HttpGet]
        public ActionResult PaymentQR()
        {
            if (TempData["TongTien"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            decimal tongTien = (decimal)TempData["TongTien"];
            string soDienThoai = TempData["SoDienThoai"]?.ToString() ?? "";
            int orderId = TempData["OrderId"] != null ? (int)TempData["OrderId"] : 0;

            // Cấu hình tài khoản ngân hàng VietQR
            string bankId = "MB";
            string accountNo = "0363420862";
            string accountName = "NGUYEN ANH KIET";
            string addInfo = $"HUFLIT HD{orderId} {soDienThoai}";

            ViewBag.TongTien = tongTien;
            ViewBag.SoDienThoai = soDienThoai;
            ViewBag.OrderId = orderId;
            ViewBag.QrCodeUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-compact2.png?amount={(int)tongTien}&addInfo={Uri.EscapeDataString(addInfo)}&accountName={Uri.EscapeDataString(accountName)}";

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