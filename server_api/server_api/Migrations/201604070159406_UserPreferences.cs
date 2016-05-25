namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPreferences : DbMigration
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
            
            CreateTable(
                "dbo.UserPreferencesParameters",
                c => new
                    {
                        UserPreferences_Id = c.String(nullable: false, maxLength: 128),
                        Parameter_Name = c.String(nullable: false, maxLength: 30),
                        Parameter_Unit = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => new { t.UserPreferences_Id, t.Parameter_Name, t.Parameter_Unit })
                .ForeignKey("dbo.UserPreferences", t => t.UserPreferences_Id, cascadeDelete: true)
                .ForeignKey("dbo.Parameters", t => new { t.Parameter_Name, t.Parameter_Unit }, cascadeDelete: true)
                .Index(t => t.UserPreferences_Id)
                .Index(t => new { t.Parameter_Name, t.Parameter_Unit });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPreferences", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserPreferencesParameters", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.UserPreferencesParameters", "UserPreferences_Id", "dbo.UserPreferences");
            DropIndex("dbo.UserPreferencesParameters", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.UserPreferencesParameters", new[] { "UserPreferences_Id" });
            DropIndex("dbo.UserPreferences", new[] { "User_Id" });
            DropTable("dbo.UserPreferencesParameters");
            DropTable("dbo.UserPreferences");
        }
    }
}
