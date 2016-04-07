namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reset : DbMigration
    {
        public override void Up()
        {
            
            CreateTable(
                "dbo.Dailies",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        Station_Id = c.String(nullable: false, maxLength: 32),
                        MaxCategory = c.Int(nullable: false),
                        MinCategory = c.Int(nullable: false),
                        MaxAQI = c.Int(nullable: false),
                        AvgAQI = c.Double(nullable: false),
                        MinAQI = c.Int(nullable: false),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                        MaxParameter_Name = c.String(maxLength: 30),
                        MaxParameter_Unit = c.String(maxLength: 30),
                        MinParameter_Name = c.String(maxLength: 30),
                        MinParameter_Unit = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => new { t.Date, t.Station_Id })
                .ForeignKey("dbo.Stations", t => t.Station_Id)
                .ForeignKey("dbo.Parameters", t => new { t.MaxParameter_Name, t.MaxParameter_Unit })
                .ForeignKey("dbo.Parameters", t => new { t.MinParameter_Name, t.MinParameter_Unit })
                .Index(t => t.Station_Id)
                .Index(t => new { t.MaxParameter_Name, t.MaxParameter_Unit })
                .Index(t => new { t.MinParameter_Name, t.MinParameter_Unit });
            
            CreateTable(
                "dbo.Parameters",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 30),
                        Unit = c.String(nullable: false, maxLength: 30),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.Name, t.Unit });
            
            CreateTable(
                "dbo.DataPoints",
                c => new
                    {
                        Time = c.DateTime(nullable: false),
                        Station_Id = c.String(nullable: false, maxLength: 32),
                        Parameter_Name = c.String(nullable: false, maxLength: 30),
                        Parameter_Unit = c.String(nullable: false, maxLength: 30),
                        SerialNumber = c.Int(nullable: false, identity: true),
                        Location = c.Geography(),
                        Value = c.Double(nullable: false),
                        Category = c.Int(nullable: false),
                        AQI = c.Int(nullable: false),
                        Indoor = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.Time, t.Station_Id, t.Parameter_Name, t.Parameter_Unit })
                .ForeignKey("dbo.Stations", t => t.Station_Id)
                .ForeignKey("dbo.Parameters", t => new { t.Parameter_Name, t.Parameter_Unit })
                .Index(t => t.Station_Id)
                .Index(t => new { t.Parameter_Name, t.Parameter_Unit });
            
            CreateTable(
                "dbo.Stations",
                c => new
                    {
                        Parameter_Name = c.String(maxLength: 30),
                        Parameter_Unit = c.String(maxLength: 30),
                        Id = c.String(nullable: false, maxLength: 32),
                        User_Id = c.String(nullable: false, maxLength: 128),
                        AQI = c.Int(nullable: false),
                        Location = c.Geography(),
                        Indoor = c.Boolean(nullable: false),
                        Agency = c.String(maxLength: 100),
                        Name = c.String(maxLength: 320),
                        Purpose = c.String(maxLength: 1000),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 10),
                        Postal = c.String(maxLength: 10),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                        StationGroup_Name = c.String(maxLength: 20),
                        StationGroup_Email = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Parameters", t => new { t.Parameter_Name, t.Parameter_Unit })
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.StationGroups", t => new { t.StationGroup_Name, t.StationGroup_Email })
                .Index(t => new { t.Parameter_Name, t.Parameter_Unit })
                .Index(t => t.User_Id)
                .Index(t => new { t.StationGroup_Name, t.StationGroup_Email });
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(nullable: false, maxLength: 256),
                        FirstName = c.String(maxLength: 20),
                        LastName = c.String(maxLength: 20),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(maxLength: 500),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(maxLength: 50),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
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
                .PrimaryKey(t => new { t.Name, t.Email })
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Keywords",
                c => new
                    {
                        keyword = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                        FrequentlyAskedQuestion_Id = c.Int(),
                    })
                .PrimaryKey(t => t.keyword)
                .ForeignKey("dbo.FrequentlyAskedQuestions", t => t.FrequentlyAskedQuestion_Id)
                .Index(t => t.FrequentlyAskedQuestion_Id);
            
            CreateTable(
                "dbo.FrequentlyAskedQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Question = c.String(),
                        Answer = c.String(),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                        Section_Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sections", t => t.Section_Name)
                .Index(t => t.Section_Name);
            
            CreateTable(
                "dbo.Sections",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTime(),
                        DateModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.FrequentlyAskedQuestions", "Section_Name", "dbo.Sections");
            DropForeignKey("dbo.Keywords", "FrequentlyAskedQuestion_Id", "dbo.FrequentlyAskedQuestions");
            DropForeignKey("dbo.StationGroups", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Stations", new[] { "StationGroup_Name", "StationGroup_Email" }, "dbo.StationGroups");
            DropForeignKey("dbo.Dailies", new[] { "MinParameter_Name", "MinParameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.Dailies", new[] { "MaxParameter_Name", "MaxParameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.DataPoints", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.Stations", "User_Id", "dbo.Users");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.Users");
            DropForeignKey("dbo.Stations", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.DataPoints", "Station_Id", "dbo.Stations");
            DropForeignKey("dbo.Dailies", "Station_Id", "dbo.Stations");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.FrequentlyAskedQuestions", new[] { "Section_Name" });
            DropIndex("dbo.Keywords", new[] { "FrequentlyAskedQuestion_Id" });
            DropIndex("dbo.StationGroups", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.Stations", new[] { "StationGroup_Name", "StationGroup_Email" });
            DropIndex("dbo.Stations", new[] { "User_Id" });
            DropIndex("dbo.Stations", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.DataPoints", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.DataPoints", new[] { "Station_Id" });
            DropIndex("dbo.Dailies", new[] { "MinParameter_Name", "MinParameter_Unit" });
            DropIndex("dbo.Dailies", new[] { "MaxParameter_Name", "MaxParameter_Unit" });
            DropIndex("dbo.Dailies", new[] { "Station_Id" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Sections");
            DropTable("dbo.FrequentlyAskedQuestions");
            DropTable("dbo.Keywords");
            DropTable("dbo.StationGroups");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.Stations");
            DropTable("dbo.DataPoints");
            DropTable("dbo.Parameters");
            DropTable("dbo.Dailies");
        }
    }
}
