namespace HuflitRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        TinhThanh = c.String(nullable: false, maxLength: 100),
                        QuanHuyen = c.String(nullable: false, maxLength: 100),
                        PhuongXa = c.String(nullable: false, maxLength: 100),
                        DiaChiCuThe = c.String(nullable: false, maxLength: 200),
                        MacDinh = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HoTen = c.String(nullable: false, maxLength: 100),
                        SoDienThoai = c.String(nullable: false, maxLength: 15),
                        Email = c.String(nullable: false, maxLength: 100),
                        MatKhauHash = c.String(nullable: false),
                        DiemTichLuy = c.Int(nullable: false),
                        NgayTao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.SoDienThoai, unique: true, name: "IX_User_SoDienThoai")
                .Index(t => t.Email, unique: true, name: "IX_User_Email");
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        NgayDat = c.DateTime(nullable: false),
                        HoTenNguoiNhan = c.String(nullable: false, maxLength: 100),
                        SoDienThoai = c.String(nullable: false, maxLength: 15),
                        Email = c.String(maxLength: 100),
                        PhuongThucNhan = c.String(nullable: false, maxLength: 30),
                        DiaChiGiao = c.String(maxLength: 400),
                        GhiChu = c.String(maxLength: 200),
                        TamTinh = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PhiVanChuyen = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiamGia = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TongCong = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TrangThai = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        TenMon = c.String(nullable: false, maxLength: 150),
                        DonGia = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SoLuong = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ten = c.String(nullable: false, maxLength: 150),
                        MoTa = c.String(maxLength: 500),
                        Gia = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HinhAnh = c.String(maxLength: 300),
                        BanChay = c.Boolean(nullable: false),
                        DangBan = c.Boolean(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ten = c.String(nullable: false, maxLength: 100),
                        MoTa = c.String(maxLength: 300),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        PhuongThuc = c.String(nullable: false, maxLength: 30),
                        SoTien = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TrangThai = c.String(nullable: false, maxLength: 30),
                        MaGiaoDich = c.String(maxLength: 100),
                        NgayTao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "UserId", "dbo.Users");
            DropForeignKey("dbo.Payments", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Addresses", "UserId", "dbo.Users");
            DropIndex("dbo.Payments", new[] { "OrderId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.OrderItems", new[] { "ProductId" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.Users", "IX_User_Email");
            DropIndex("dbo.Users", "IX_User_SoDienThoai");
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropTable("dbo.Payments");
            DropTable("dbo.Categories");
            DropTable("dbo.Products");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
            DropTable("dbo.Users");
            DropTable("dbo.Addresses");
        }
    }
}
