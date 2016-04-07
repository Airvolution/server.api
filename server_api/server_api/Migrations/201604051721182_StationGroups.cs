namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StationGroups : DbMigration
    {
        public override void Up()
        {
            
            DropForeignKey("dbo.Stations", new[] { "StationGroup_Name", "StationGroup_Email" }, "dbo.StationGroups");
            DropForeignKey("dbo.StationGroups", "User_Id", "dbo.Users");
            DropIndex("dbo.Stations", new[] { "StationGroup_Name", "StationGroup_Email" });
            DropIndex("dbo.StationGroups", new[] { "User_Id" });
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Owner_Id = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Owner_Id, cascadeDelete: true)
                .Index(t => t.Owner_Id);
            DropTable("dbo.StationGroups");
            CreateTable(
                "dbo.StationGroups",
                c => new
                    {
                        Group_Id = c.Int(nullable: false),
                        Station_Id = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => new { t.Group_Id, t.Station_Id })
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: true)
                .ForeignKey("dbo.Stations", t => t.Station_Id, cascadeDelete: true)
                .Index(t => t.Group_Id)
                .Index(t => t.Station_Id);
            
            DropColumn("dbo.Stations", "StationGroup_Name");
            DropColumn("dbo.Stations", "StationGroup_Email");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StationGroups", "Station_Id", "dbo.Stations");
            DropForeignKey("dbo.StationGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.Groups", "Owner_Id", "dbo.Users");
            DropIndex("dbo.StationGroups", new[] { "Station_Id" });
            DropIndex("dbo.StationGroups", new[] { "Group_Id" });
            DropIndex("dbo.Groups", new[] { "Owner_Id" });
            DropTable("dbo.StationGroups");
            DropTable("dbo.Groups");
            CreateTable(
                "dbo.StationGroups",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 20),
                        Email = c.String(nullable: false, maxLength: 100),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Name, t.Email });
            
            AddColumn("dbo.Stations", "StationGroup_Email", c => c.String(maxLength: 100));
            AddColumn("dbo.Stations", "StationGroup_Name", c => c.String(maxLength: 20));
            CreateIndex("dbo.StationGroups", "User_Id");
            CreateIndex("dbo.Stations", new[] { "StationGroup_Name", "StationGroup_Email" });
            AddForeignKey("dbo.StationGroups", "User_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.Stations", new[] { "StationGroup_Name", "StationGroup_Email" }, "dbo.StationGroups", new[] { "Name", "Email" });
        }
    }
}
