using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HuflitRestaurant.Models;
using Newtonsoft.Json.Linq;

namespace HuflitRestaurant.Services
{
    public class AIService
    {
        private static readonly string ApiKey =
            (ConfigurationManager.AppSettings["GeminiApiKey"] ?? "").Trim();

        // Model chính, rồi các model dự phòng (cách nhau bằng dấu phẩy trong Web.config)
        private static readonly string[] Models = BuildModelList();

        private static readonly HttpClient Http;

        static AIService()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        private static string[] BuildModelList()
        {
            var list = new List<string>();

            string main = ConfigurationManager.AppSettings["GeminiModel"];
            list.Add(string.IsNullOrWhiteSpace(main) ? "gemini-3.8-flash" : main.Trim());

            string fallback = ConfigurationManager.AppSettings["GeminiFallbackModels"];
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                list.AddRange(fallback.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(m => m.Trim())
                                      .Where(m => m.Length > 0));
            }

            return list.Distinct().ToArray();
        }

        private static bool IsRetryable(int code)
        {
            return code == 429 || code == 500 || code == 502 || code == 503 || code == 504;
        }

        // Tạo nội dung gửi lên Gemini. Với dòng Gemini 3: giảm mức suy nghĩ xuống "low" để trả lời nhanh
        private static string BuildRequestJson(string systemPrompt, string userMessage, bool useThinkingLevel)
        {
            var generationConfig = new JObject
            {
                ["maxOutputTokens"] = 512,
                ["temperature"] = 0.6
            };

            if (useThinkingLevel)
            {
                generationConfig["thinkingConfig"] = new JObject { ["thinkingLevel"] = "low" };
            }

            var request = new JObject
            {
                ["systemInstruction"] = new JObject
                {
                    ["parts"] = new JArray(new JObject { ["text"] = systemPrompt })
                },
                ["contents"] = new JArray(new JObject
                {
                    ["role"] = "user",
                    ["parts"] = new JArray(new JObject { ["text"] = userMessage })
                }),
                ["generationConfig"] = generationConfig
            };

            return request.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static async Task<string> GetAIResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                return "Chưa cấu hình GeminiApiKey trong Web.config.";
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Bạn muốn hỏi gì về HUFLIT RESTAURANT ạ?";
            }

            string systemPrompt = BuildSystemPrompt();
            string lastDetail = "";

            foreach (string model in Models)
            {
                bool useThinking = model.StartsWith("gemini-3", StringComparison.OrdinalIgnoreCase);

                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        string url = "https://generativelanguage.googleapis.com/v1beta/models/"
                                     + model + ":generateContent?key=" + ApiKey;

                        string json = BuildRequestJson(systemPrompt, userMessage, useThinking);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await Http.PostAsync(url, content);
                        string body = await response.Content.ReadAsStringAsync();

                        Debug.WriteLine("Gemini " + model + ": " + (int)response.StatusCode
                                        + " sau " + sw.ElapsedMilliseconds + " ms");

                        if (response.IsSuccessStatusCode)
                        {
                            string reply = null;
                            try
                            {
                                reply = (string)JObject.Parse(body)["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];
                            }
                            catch { }

                            if (!string.IsNullOrWhiteSpace(reply))
                            {
                                return reply.Trim();
                            }

                            lastDetail = model + ": phản hồi rỗng";
                            break; // thử model tiếp theo
                        }

                        int code = (int)response.StatusCode;
                        lastDetail = model + " " + code + ": " + ExtractError(body);
                        Debug.WriteLine("Gemini lỗi " + lastDetail);

                        // Model không nhận thinkingLevel -> bỏ tham số này rồi gọi lại (chỉ xảy ra 1 lần)
                        if (code == 400 && useThinking)
                        {
                            useThinking = false;
                            attempt--;
                            continue;
                        }

                        if (!IsRetryable(code))
                        {
                            break; // lỗi không tạm thời (404, 400...) -> sang model khác
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Hết thời gian chờ: không thử lại cùng model (tránh chờ gấp đôi), sang model khác
                        lastDetail = model + ": hết thời gian chờ";
                        Debug.WriteLine("Gemini " + lastDetail);
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastDetail = model + ": " + ex.GetType().Name;
                        Debug.WriteLine("Gemini lỗi kết nối: " + ex.Message);
                    }

                    if (attempt < 2)
                    {
                        await Task.Delay(1000);
                    }
                }
            }

            string friendly = "Hệ thống trợ lý AI đang quá tải. Bạn vui lòng thử lại sau ít phút hoặc gọi hotline 0123 456 789 nhé.";
#if DEBUG
            return friendly + " [DEBUG: " + lastDetail + "]";
#else
            return friendly;
#endif
        }

        private static string ExtractError(string body)
        {
            try
            {
                return (string)JObject.Parse(body)["error"]?["message"] ?? body;
            }
            catch
            {
                return body;
            }
        }

        // Prompt hệ thống: có thực đơn thật lấy từ database
        private static string BuildSystemPrompt()
        {
            string menu = "";
            try
            {
                using (var db = new AppDbContext())
                {
                    var items = db.Products
                                  .Where(p => p.DangBan)
                                  .OrderBy(p => p.CategoryId)
                                  .ThenBy(p => p.Id)
                                  .Select(p => new { p.Ten, p.MoTa, p.Gia })
                                  .ToList();

                    var sb = new StringBuilder();
                    foreach (var p in items)
                    {
                        sb.AppendLine("- " + p.Ten + " (" + p.MoTa + "): "
                                      + p.Gia.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) + " đồng");
                    }
                    menu = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Không đọc được thực đơn: " + ex.Message);
            }

            return
"Bạn là trợ lý AI chăm sóc khách hàng của HUFLIT RESTAURANT, nhà hàng món ăn Hàn Quốc.\n" +
"Nhiệm vụ: trả lời thân thiện, lịch sự, ngắn gọn (dưới 100 từ), bằng tiếng Việt.\n" +
"Thông tin nhà hàng:\n" +
"- Địa chỉ: 123 Đường ABC, Quận X, TP. Hồ Chí Minh.\n" +
"- Hotline: 0123 456 789.\n" +
"- Giờ mở cửa: 08:00 - 22:00 hằng ngày.\n" +
"- Nhận hàng: giao tận nơi hoặc đến lấy tại quán.\n" +
"- Thanh toán: COD, MoMo, ZaloPay, chuyển khoản (mã QR).\n" +
"Thực đơn hiện có:\n" + menu +
"Quy tắc: chỉ tư vấn dựa trên thông tin ở trên. Nếu không biết hoặc ngoài phạm vi, " +
"hãy mời khách gọi hotline. Không bịa món, giá hay khuyến mãi.";
        }
    }
}