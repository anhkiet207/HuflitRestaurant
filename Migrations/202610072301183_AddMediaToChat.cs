namespace HuflitRestaurant.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMediaToChat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChatMessages", "MessageType", c => c.String());
            AddColumn("dbo.ChatMessages", "MediaUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChatMessages", "MediaUrl");
            DropColumn("dbo.ChatMessages", "MessageType");
        }
    }
}
