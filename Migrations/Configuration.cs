namespace HuflitRestaurant.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using HuflitRestaurant.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AppDbContext context)
        {
            // 1. Danh mục
            context.Categories.AddOrUpdate(c => c.Ten,
                new Category { Ten = "Tokbokki & Bánh gạo", MoTa = "Bánh gạo cay, sốt ngọt, ăn vặt kiểu Hàn" },
                new Category { Ten = "Cơm trộn & Kimbap", MoTa = "Cơm trộn, kimbap truyền thống" },
                new Category { Ten = "Món nướng", MoTa = "Thịt nướng kiểu Hàn Quốc" },
                new Category { Ten = "Canh & Lẩu", MoTa = "Canh kimchi, lẩu quân đội, canh đậu hũ" },
                new Category { Ten = "Gà rán & Ăn vặt", MoTa = "Gà rán sốt Hàn và các món ăn vặt" },
                new Category { Ten = "Mì Hàn Quốc", MoTa = "Mì tương đen, mì cay, miến trộn" }
            );
            context.SaveChanges();

            // 2. Lấy Id danh mục
            int tok = context.Categories.First(c => c.Ten == "Tokbokki & Bánh gạo").Id;
            int com = context.Categories.First(c => c.Ten == "Cơm trộn & Kimbap").Id;
            int nuong = context.Categories.First(c => c.Ten == "Món nướng").Id;
            int canh = context.Categories.First(c => c.Ten == "Canh & Lẩu").Id;
            int ga = context.Categories.First(c => c.Ten == "Gà rán & Ăn vặt").Id;
            int mi = context.Categories.First(c => c.Ten == "Mì Hàn Quốc").Id;

            // 3. Món ăn
            context.Products.AddOrUpdate(p => p.Ten,
                new Product { Ten = "Tteokbokki", MoTa = "Bánh gạo cay sốt ngọt", Gia = 98000, HinhAnh = "/Content/images/tteokbokki.jpg", CategoryId = tok, BanChay = true, DangBan = true },
                new Product { Ten = "Tteokbokki phô mai", MoTa = "Bánh gạo cay phủ phô mai kéo sợi", Gia = 119000, HinhAnh = "/Content/images/tteokbokki-pho-mai.jpg", CategoryId = tok, BanChay = false, DangBan = true },
                new Product { Ten = "Rabokki", MoTa = "Bánh gạo cay kết hợp mì ramyun", Gia = 109000, HinhAnh = "/Content/images/rabokki.jpg", CategoryId = tok, BanChay = false, DangBan = true },

                new Product { Ten = "Bibimbap", MoTa = "Cơm trộn kiểu Hàn", Gia = 148000, HinhAnh = "/Content/images/bibimbap.jpg", CategoryId = com, BanChay = true, DangBan = true },
                new Product { Ten = "Kimbap truyền thống", MoTa = "Cơm cuộn rong biển nhân rau củ, trứng, xúc xích", Gia = 69000, HinhAnh = "/Content/images/kimbap.jpg", CategoryId = com, BanChay = true, DangBan = true },
                new Product { Ten = "Kimbap chiên", MoTa = "Kimbap chiên giòn ăn kèm sốt", Gia = 79000, HinhAnh = "/Content/images/kimbap-chien.jpg", CategoryId = com, BanChay = false, DangBan = true },

                new Product { Ten = "Samgyeopsal", MoTa = "Ba chỉ heo nướng, ăn kèm rau và sốt ssamjang", Gia = 189000, HinhAnh = "/Content/images/samgyeopsal.jpg", CategoryId = nuong, BanChay = false, DangBan = true },
                new Product { Ten = "Bulgogi", MoTa = "Bò ướp nướng kiểu Hàn", Gia = 169000, HinhAnh = "/Content/images/bulgogi.jpg", CategoryId = nuong, BanChay = false, DangBan = true },
                new Product { Ten = "Galbi", MoTa = "Sườn bò nướng ướp sốt đặc trưng", Gia = 219000, HinhAnh = "/Content/images/galbi.jpg", CategoryId = nuong, BanChay = false, DangBan = true },

                new Product { Ten = "Kimchi Jjigae", MoTa = "Canh kimchi đậu hũ", Gia = 88000, HinhAnh = "/Content/images/kimchi-jjigae.jpg", CategoryId = canh, BanChay = true, DangBan = true },
                new Product { Ten = "Sundubu Jjigae", MoTa = "Canh đậu hũ non cay", Gia = 95000, HinhAnh = "/Content/images/sundubu-jjigae.jpg", CategoryId = canh, BanChay = false, DangBan = true },
                new Product { Ten = "Budae Jjigae", MoTa = "Lẩu quân đội với xúc xích, mì, phô mai", Gia = 199000, HinhAnh = "/Content/images/budae-jjigae.jpg", CategoryId = canh, BanChay = false, DangBan = true },

                new Product { Ten = "Gà rán sốt Yangnyeom", MoTa = "Gà rán giòn phủ sốt cay ngọt", Gia = 129000, HinhAnh = "/Content/images/ga-yangnyeom.jpg", CategoryId = ga, BanChay = false, DangBan = true },
                new Product { Ten = "Gà rán sốt mật ong", MoTa = "Gà rán giòn sốt mật ong tỏi", Gia = 129000, HinhAnh = "/Content/images/ga-mat-ong.jpg", CategoryId = ga, BanChay = false, DangBan = true },

                new Product { Ten = "Jajangmyeon", MoTa = "Mì tương đen Hàn Quốc", Gia = 89000, HinhAnh = "/Content/images/jajangmyeon.jpg", CategoryId = mi, BanChay = false, DangBan = true },
                new Product { Ten = "Japchae", MoTa = "Miến trộn rau củ và thịt bò", Gia = 99000, HinhAnh = "/Content/images/japchae.jpg", CategoryId = mi, BanChay = false, DangBan = true },
                new Product { Ten = "Ramyun phô mai", MoTa = "Mì cay Hàn Quốc ăn kèm phô mai", Gia = 69000, HinhAnh = "/Content/images/ramyun.jpg", CategoryId = mi, BanChay = false, DangBan = true }
            );
            context.SaveChanges();
        }
    }
}