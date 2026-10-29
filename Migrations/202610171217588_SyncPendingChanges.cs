namespace HuflitRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncPendingChanges : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Products", "ViTriAnh");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "ViTriAnh", c => c.String());
        }
    }
}
