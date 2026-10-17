namespace HuflitRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncModelChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ViTriAnh", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "ViTriAnh");
        }
    }
}
