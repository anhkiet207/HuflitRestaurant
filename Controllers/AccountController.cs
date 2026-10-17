using HuflitRestaurant.Models;
using HuflitRestaurant.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HuflitRestaurant.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existUser = db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                    return View(model);
                }

                var user = new User
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    MatKhau = model.MatKhau, // Trong thực tế nên mã hóa Password
                    VaiTro = "Customer",
                    NgayTao = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();

                // Đăng ký xong tự động đăng nhập
                Session["UserId"] = user.Id;
                Session["UserName"] = user.HoTen;
                Session["UserEmail"] = user.Email;

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return View();
            }

            string cleanEmail = email.Trim().ToLower();
            string cleanPassword = password.Trim();

            // Tìm user trong Database
            var user = db.Users.FirstOrDefault(u => u.Email.Trim().ToLower() == cleanEmail && u.MatKhau == cleanPassword);

            if (user != null)
            {
                // Gán đầy đủ Session
                Session["UserId"] = user.Id;
                Session["UserName"] = user.HoTen;
                Session["UserRole"] = user.VaiTro;

                // Nếu là Admin -> Chuyển hướng tới trang Quản lý đơn hàng
                if (!string.IsNullOrEmpty(user.VaiTro) && user.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Orders", "Admin");
                }

                // Nếu là Khách hàng -> Chuyển hướng về Trang chủ
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
            return View();
        }
        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public ActionResult Profile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            var orders = db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.NgayDat).ToList();

            var vm = new UserProfileViewModel
            {
                User = user,
                Orders = orders
            };

            return View(vm);
        }

        // GET: Account/OrderDetail/5
        public ActionResult OrderDetail(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin đơn hàng cùng danh sách món ăn chi tiết
            var order = db.Orders.FirstOrDefault(o => o.Id == id.Value);
            if (order == null)
            {
                return HttpNotFound();
            }

            var orderItems = db.OrderItems.Where(i => i.OrderId == order.Id).ToList();

            ViewBag.Order = order;
            ViewBag.OrderItems = orderItems;

            return View();
        }
        // GET: Account/CustomerSupport
        public ActionResult CustomerSupport()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];
            var messages = db.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            return View(messages);
        }

        // POST: Account/SendCustomerSupportMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendCustomerSupportMessage(string message)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserId"];
            if (!string.IsNullOrWhiteSpace(message))
            {
                var chatMsg = new ChatMessage
                {
                    UserId = userId,
                    SenderRole = "User",
                    Message = message.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.ChatMessages.Add(chatMsg);
                db.SaveChanges();
            }

            return RedirectToAction("CustomerSupport");
        }
        // API lấy danh sách tin nhắn dạng JSON cho Khách hàng
        // API lấy tin nhắn khách hàng
        [HttpGet]
        public ActionResult GetCustomerMessagesJson()
        {
            if (Session["UserId"] == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            int userId = Convert.ToInt32(Session["UserId"]);
            var messages = db.ChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ToList()
                .Select(m => new {
                    m.Id,
                    m.SenderRole,
                    m.Message,
                    m.MessageType,
                    m.MediaUrl,
                    CreatedAt = m.CreatedAt.ToString("HH:mm")
                })
                .ToList();

            return Json(new { success = true, messages = messages }, JsonRequestBehavior.AllowGet);
        }

        // API gửi ảnh/video từ Khách hàng
        [HttpPost]
        public ActionResult SendCustomerMediaMessageAjax(System.Web.HttpPostedFileBase file)
        {
            if (Session["UserId"] == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            if (file != null && file.ContentLength > 0)
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                string ext = System.IO.Path.GetExtension(file.FileName).ToLower();
                string messageType = "Image";

                if (ext == ".mp4" || ext == ".webm" || ext == ".mov")
                {
                    messageType = "Video";
                }

                string fileName = Guid.NewGuid().ToString() + ext;
                string folderPath = Server.MapPath("~/Content/Uploads/Chat/");
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                string filePath = System.IO.Path.Combine(folderPath, fileName);
                file.SaveAs(filePath);

                var chatMsg = new ChatMessage
                {
                    UserId = userId,
                    SenderRole = "User",
                    Message = messageType == "Image" ? "[Hình ảnh]" : "[Video]",
                    MessageType = messageType,
                    MediaUrl = "/Content/Uploads/Chat/" + fileName,
                    CreatedAt = DateTime.Now
                };

                db.ChatMessages.Add(chatMsg);
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<ActionResult> SendCustomerSupportMessageAjax(string message)
        {
            if (Session["UserId"] == null)
                return Json(new { success = false, message = "Chưa đăng nhập" });

            if (!string.IsNullOrWhiteSpace(message))
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                // 1. Lưu tin nhắn của Khách hàng
                var userMsg = new ChatMessage
                {
                    UserId = userId,
                    SenderRole = "User",
                    Message = message.Trim(),
                    MessageType = "Text",
                    CreatedAt = DateTime.Now
                };
                db.ChatMessages.Add(userMsg);
                db.SaveChanges();

                // 2. Tự động gọi Gemini AI trả lời
                string aiReply = await AIService.GetAIResponseAsync(message.Trim());

                // 3. Lưu câu trả lời của AI vào CSDL với tư cách Admin Support
                var aiMsg = new ChatMessage
                {
                    UserId = userId,
                    SenderRole = "Admin", // Hoặc đặt "AI" nếu muốn phân biệt
                    Message = aiReply,
                    MessageType = "Text",
                    CreatedAt = DateTime.Now
                };
                db.ChatMessages.Add(aiMsg);
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
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