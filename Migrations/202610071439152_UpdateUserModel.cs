namespace HuflitRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserModel : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Users", "IX_User_SoDienThoai");
            DropIndex("dbo.Users", "IX_User_Email");
            AddColumn("dbo.Users", "MatKhau", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Users", "VaiTro", c => c.String(maxLength: 20));
            AlterColumn("dbo.Users", "SoDienThoai", c => c.String(maxLength: 20));
            DropColumn("dbo.Users", "MatKhauHash");
            DropColumn("dbo.Users", "DiemTichLuy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "DiemTichLuy", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "MatKhauHash", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "SoDienThoai", c => c.String(nullable: false, maxLength: 15));
            DropColumn("dbo.Users", "VaiTro");
            DropColumn("dbo.Users", "MatKhau");
            CreateIndex("dbo.Users", "Email", unique: true, name: "IX_User_Email");
            CreateIndex("dbo.Users", "SoDienThoai", unique: true, name: "IX_User_SoDienThoai");
        }
    }
}
