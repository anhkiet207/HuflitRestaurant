using System;
using System.Data.Entity.Migrations;

namespace HuflitRestaurant.Migrations
{
    public partial class AddChatMessage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessages",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.Int(nullable: false),
                    SenderRole = c.String(),
                    Message = c.String(),
                    CreatedAt = c.DateTime(nullable: false),
                    IsRead = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ChatMessages", "UserId", "dbo.Users");
            DropIndex("dbo.ChatMessages", new[] { "UserId" });
            DropTable("dbo.ChatMessages");
        }
    }
}