using HuflitRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace HuflitRestaurant.Controllers
{
    public class AdminController : Controller
    {
        private AppDbContext db = new AppDbContext();

        private static readonly string[] ImageExts = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] VideoExts = { ".mp4", ".webm", ".mov" };

        private bool IsAdmin()
        {
            if (Session["UserId"] == null) return false;
            int userId = (int)Session["UserId"];
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            return user != null && user.VaiTro == "Admin";
        }

        // Hàm hỗ trợ tự động đọc tên Tên/Name từ Model bằng Reflection (Chống 100% lỗi CS1061)
        private string GetEntityName(object entity)
        {
            if (entity == null) return "";
            var props = entity.GetType().GetProperties();
            var nameProp = props.FirstOrDefault(p =>
              p.Name.Equals("Ten", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("TenMon", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("TenMonAn", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("TenSanPham", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("TenCategory", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("TenDanhMuc", StringComparison.OrdinalIgnoreCase) ||
              p.Name.Equals("TenLoai", StringComparison.OrdinalIgnoreCase) ||
              p.Name.StartsWith("Ten", StringComparison.OrdinalIgnoreCase) ||
              p.Name.StartsWith("Name", StringComparison.OrdinalIgnoreCase)
            );

            if (nameProp != null)
            {
                var val = nameProp.GetValue(entity, null);
                return val != null ? val.ToString() : "";
            }
            return "";
        }

        // GET: Admin/Orders
        public ActionResult Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var orders = db.Orders.OrderByDescending(o => o.NgayDat).ToList();
            return View(orders);
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var order = db.Orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.TrangThai = status;
                db.SaveChanges();
            }
            return RedirectToAction("Orders");
        }

        // POST: Admin/ResetOrders
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetOrders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var allOrderItems = db.OrderItems.ToList();
            db.OrderItems.RemoveRange(allOrderItems);

            var allOrders = db.Orders.ToList();
            db.Orders.RemoveRange(allOrders);

            db.SaveChanges();

            TempData["ResetSuccess"] = "Đã reset và xóa toàn bộ dữ liệu đơn hàng thành công!";
            return RedirectToAction("Orders");
        }

        // GET: Admin/Product
        public ActionResult Product()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var products = db.Products.OrderByDescending(p => p.Id).ToList();

            if (ViewEngines.Engines.FindView(ControllerContext, "Product", null).View != null)
            {
                return View("Product", products);
            }
            return View("Products", products);
        }

        // GET: Admin/Products
        public ActionResult Products()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var products = db.Products.OrderByDescending(p => p.Id).ToList();

            if (ViewEngines.Engines.FindView(ControllerContext, "Products", null).View != null)
            {
                return View("Products", products);
            }
            return View("Product", products);
        }

        // GET: Admin/CreateProduct
        [HttpGet]
        public ActionResult CreateProduct()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Categories = db.Categories.ToList();
            return View();
        }

        // POST: Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Product product)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (product.CategoryId == 0)
            {
                var firstCategory = db.Categories.FirstOrDefault();
                if (firstCategory != null)
                {
                    product.CategoryId = firstCategory.Id;
                }
            }

            if (ModelState.IsValid)
            {
                product.DangBan = true;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Product");
            }

            ViewBag.Categories = db.Categories.ToList();
            return View(product);
        }

        // POST: Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var product = db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                bool hasOrders = db.OrderItems.Any(i => i.ProductId == id);
                if (hasOrders)
                {
                    product.DangBan = false;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Món ăn đã từng có đơn hàng nên hệ thống đã chuyển sang trạng thái 'Ngừng bán'!";
                }
                else
                {
                    db.Products.Remove(product);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đã xóa món ăn thành công!";
                }
            }

            return RedirectToAction("Product");
        }

        // GET: Admin/Chat
        [HttpGet]
        public ActionResult Chat()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Admin/SendAdminChatMessage
        [HttpPost]
        public async Task<JsonResult> SendAdminChatMessage(string message)
        {
            if (!IsAdmin())
                return Json(new { Success = false, ErrorMessage = "Bạn không có quyền Admin!" });

            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { Success = false, ErrorMessage = "Nội dung tin nhắn không được để trống." });
            }

            try
            {
                string apiKey = ConfigurationManager.AppSettings["GeminiAPIKey"];
                string model = ConfigurationManager.AppSettings["GeminiModel"];

                if (string.IsNullOrWhiteSpace(model))
                {
                    model = "gemini-1.5-flash";
                }

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Json(new { Success = false, ErrorMessage = "Chưa cấu hình GeminiAPIKey trong web.config!" });
                }

                // Lấy dữ liệu thực đơn thực tế từ Database (Tự động nhận diện tên thuộc tính qua GetEntityName)
                var categories = db.Categories.ToList();
                var activeProducts = db.Products.Where(p => p.DangBan).ToList();

                StringBuilder menuBuilder = new StringBuilder();
                foreach (var cat in categories)
                {
                    var prodsInCat = activeProducts.Where(p => p.CategoryId == cat.Id).ToList();
                    if (prodsInCat.Any())
                    {
                        string catName = GetEntityName(cat);
                        menuBuilder.AppendLine($"\nDanh mục: {(string.IsNullOrEmpty(catName) ? "Danh mục" : catName)}");
                        foreach (var prod in prodsInCat)
                        {
                            string prodName = GetEntityName(prod);
                            menuBuilder.AppendLine($"- {(string.IsNullOrEmpty(prodName) ? "Món ăn" : prodName)}: {prod.Gia:N0} VNĐ (Mô tả: {prod.MoTa})");
                        }
                    }
                }
                string menu = menuBuilder.ToString();

                string apiEndpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                string adminSystemPrompt = "Bạn là Trợ lý AI chuyên nghiệp hỗ trợ Ban Quản lý / Admin của HUFLIT RESTAURANT, nhà hàng món ăn Hàn Quốc.\n" +
                              "Nhiệm vụ: Hỗ trợ Admin giải đáp quy trình vận hành, tư vấn thực đơn, soạn thảo nội dung ưu đãi, hoặc gợi ý câu trả lời đánh giá khách hàng.\n" +
                              "Yêu cầu: Trả lời thân thiện, lịch sự, ngắn gọn (dưới 150 từ), trình bày rõ ràng, phân đoạn xuống dòng hợp lý.\n\n" +
                              "Thông tin nhà hàng:\n" +
                              "- Địa chỉ: 123 Đường ABC, Quận X, TP. Hồ Chí Minh.\n" +
                              "- Hotline: 036 342 0862\n" +
                              "- Giờ mở cửa: 08:00 - 22:00 hằng ngày.\n" +
                              "- Nhận hàng: Giao tận nơi hoặc đến lấy tại quán.\n" +
                              "- Thanh toán: COD, MoMo, ZaloPay, chuyển khoản (mã QR).\n\n" +
                              "Thực đơn hiện có trong cơ sở dữ liệu:\n" + menu + "\n\n" +
                              "Quy tắc: Chỉ tư vấn dựa trên thông tin thực đơn ở trên. Tuyệt đối không tự bịa ra món ăn, giá bán hay khuyến mãi không có trong danh sách.";

                using (var httpClient = new HttpClient())
                {
                    var requestData = new
                    {
                        contents = new[]
                      {
              new
              {
                parts = new[]
                {
                  new { text = $"{adminSystemPrompt}\n\nLệnh/Câu hỏi từ Admin: {message}" }
                }
              }
            }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic jsonResult = JsonConvert.DeserializeObject(responseString);

                        string aiReply = jsonResult?.candidates?[0]?.content?.parts?[0]?.text;

                        if (!string.IsNullOrWhiteSpace(aiReply))
                        {
                            return Json(new { Success = true, Reply = aiReply });
                        }
                    }
                    else
                    {
                        var errorDetail = await response.Content.ReadAsStringAsync();
                        return Json(new { Success = false, ErrorMessage = $"Lỗi kết nối Gemini API ({response.StatusCode}): {errorDetail}" });
                    }
                }

                return Json(new { Success = false, ErrorMessage = "Không nhận được phản hồi từ dịch vụ AI." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, ErrorMessage = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST: Admin/SendChatMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendChatMessage(int userId, string message)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(message))
            {
                var chatMsg = new ChatMessage
                {
                    UserId = userId,
                    SenderRole = "Admin",
                    Message = message.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.ChatMessages.Add(chatMsg);
                db.SaveChanges();
            }

            return RedirectToAction("Chat", new { userId = userId });
        }

        // POST: Admin/SendChatMessageAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendChatMessageAjax(int userId, string message)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền Admin" });

            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Tin nhắn trống" });

            if (!db.Users.Any(u => u.Id == userId))
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });

            var chatMsg = new ChatMessage
            {
                UserId = userId,
                SenderRole = "Admin",
                Message = message.Trim(),
                MessageType = "Text",
                CreatedAt = DateTime.Now
            };

            db.ChatMessages.Add(chatMsg);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // API lấy danh sách tin nhắn dạng JSON cho Admin
        [HttpGet]
        public ActionResult GetMessagesJson(int userId)
        {
            if (!IsAdmin()) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var messages = db.ChatMessages
              .Where(m => m.UserId == userId)
              .OrderBy(m => m.CreatedAt)
              .ToList()
              .Select(m => new
              {
                  m.Id,
                  m.SenderRole,
                  Message = HttpUtility.HtmlEncode(m.Message),
                  m.MessageType,
                  m.MediaUrl,
                  CreatedAt = m.CreatedAt.ToString("HH:mm")
              })
              .ToList();

            return Json(new { success = true, messages = messages }, JsonRequestBehavior.AllowGet);
        }

        // API gửi ảnh/video từ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMediaMessageAjax(int userId, HttpPostedFileBase file)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền Admin" });

            if (file == null || file.ContentLength <= 0)
                return Json(new { success = false, message = "File không hợp lệ" });

            string ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            string messageType;

            if (ImageExts.Contains(ext)) messageType = "Image";
            else if (VideoExts.Contains(ext)) messageType = "Video";
            else return Json(new { success = false, message = "Chỉ hỗ trợ ảnh (jpg, png, gif, webp) hoặc video (mp4, webm, mov)" });

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
                SenderRole = "Admin",
                Message = messageType == "Image" ? "[Hình ảnh]" : "[Video]",
                MessageType = messageType,
                MediaUrl = "/Content/Uploads/Chat/" + fileName,
                CreatedAt = DateTime.Now
            };

            db.ChatMessages.Add(chatMsg);
            db.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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