namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ManyToManyUserPrefs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Parameters", "UserPreferences_User_Id", "dbo.UserPreferences");
            DropIndex("dbo.Parameters", new[] { "UserPreferences_User_Id" });
            CreateTable(
                "dbo.ParameterUserPreferences",
                c => new
                    {
                        Parameter_Name = c.String(nullable: false, maxLength: 30),
                        Parameter_Unit = c.String(nullable: false, maxLength: 30),
                        UserPreferences_User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Parameter_Name, t.Parameter_Unit, t.UserPreferences_User_Id })
                .ForeignKey("dbo.Parameters", t => new { t.Parameter_Name, t.Parameter_Unit }, cascadeDelete: true)
                .ForeignKey("dbo.UserPreferences", t => t.UserPreferences_User_Id, cascadeDelete: true)
                .Index(t => new { t.Parameter_Name, t.Parameter_Unit })
                .Index(t => t.UserPreferences_User_Id);
            
            DropColumn("dbo.Parameters", "UserPreferences_User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Parameters", "UserPreferences_User_Id", c => c.String(maxLength: 128));
            DropForeignKey("dbo.ParameterUserPreferences", "UserPreferences_User_Id", "dbo.UserPreferences");
            DropForeignKey("dbo.ParameterUserPreferences", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropIndex("dbo.ParameterUserPreferences", new[] { "UserPreferences_User_Id" });
            DropIndex("dbo.ParameterUserPreferences", new[] { "Parameter_Name", "Parameter_Unit" });
            DropTable("dbo.ParameterUserPreferences");
            CreateIndex("dbo.Parameters", "UserPreferences_User_Id");
            AddForeignKey("dbo.Parameters", "UserPreferences_User_Id", "dbo.UserPreferences", "User_Id");
        }
    }
}
