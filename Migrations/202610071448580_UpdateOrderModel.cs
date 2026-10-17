namespace HuflitRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrderModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TongTien", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "SoDienThoaiGiao", c => c.String(maxLength: 20));
            AlterColumn("dbo.Orders", "DiaChiGiao", c => c.String(maxLength: 255));
            AlterColumn("dbo.Orders", "GhiChu", c => c.String());
            AlterColumn("dbo.Orders", "TrangThai", c => c.String(maxLength: 50));
            DropColumn("dbo.Orders", "HoTenNguoiNhan");
            DropColumn("dbo.Orders", "SoDienThoai");
            DropColumn("dbo.Orders", "Email");
            DropColumn("dbo.Orders", "PhuongThucNhan");
            DropColumn("dbo.Orders", "TamTinh");
            DropColumn("dbo.Orders", "PhiVanChuyen");
            DropColumn("dbo.Orders", "GiamGia");
            DropColumn("dbo.Orders", "TongCong");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "TongCong", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "GiamGia", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "PhiVanChuyen", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "TamTinh", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "PhuongThucNhan", c => c.String(nullable: false, maxLength: 30));
            AddColumn("dbo.Orders", "Email", c => c.String(maxLength: 100));
            AddColumn("dbo.Orders", "SoDienThoai", c => c.String(nullable: false, maxLength: 15));
            AddColumn("dbo.Orders", "HoTenNguoiNhan", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Orders", "TrangThai", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.Orders", "GhiChu", c => c.String(maxLength: 200));
            AlterColumn("dbo.Orders", "DiaChiGiao", c => c.String(maxLength: 400));
            DropColumn("dbo.Orders", "SoDienThoaiGiao");
            DropColumn("dbo.Orders", "TongTien");
        }
    }
}
