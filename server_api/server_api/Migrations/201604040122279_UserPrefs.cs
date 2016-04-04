namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPrefs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserPreferences",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        DefaultMapMode = c.String(),
                        DefaultDownloadFormat = c.String(),
                        DefaultStationId = c.String(),
                    })
                .PrimaryKey(t => t.User_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.Parameters", "UserPreferences_User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Parameters", "UserPreferences_User_Id");
            AddForeignKey("dbo.Parameters", "UserPreferences_User_Id", "dbo.UserPreferences", "User_Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPreferences", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Parameters", "UserPreferences_User_Id", "dbo.UserPreferences");
            DropIndex("dbo.UserPreferences", new[] { "User_Id" });
            DropIndex("dbo.Parameters", new[] { "UserPreferences_User_Id" });
            DropColumn("dbo.Parameters", "UserPreferences_User_Id");
            DropTable("dbo.UserPreferences");
        }
    }
}
