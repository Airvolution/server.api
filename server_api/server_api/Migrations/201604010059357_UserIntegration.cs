namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserIntegration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Stations", "User_Id", "dbo.Users");
            DropForeignKey("dbo.StationGroups", "User_Id", "dbo.Users");
            DropIndex("dbo.Stations", new[] { "User_Id" });
            DropIndex("dbo.StationGroups", new[] { "User_Id" });
            //DropPrimaryKey("dbo.Users");



            AddColumn("dbo.Users", "EmailConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "PasswordHash", c => c.String(maxLength: 500));
            AddColumn("dbo.Users", "SecurityStamp", c => c.String());
            AddColumn("dbo.Users", "PhoneNumber", c => c.String(maxLength: 50));
            AddColumn("dbo.Users", "PhoneNumberConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "TwoFactorEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "LockoutEndDateUtc", c => c.DateTime());
            AddColumn("dbo.Users", "LockoutEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "AccessFailedCount", c => c.Int(nullable: false));
            AlterColumn("dbo.Stations", "User_Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Users", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Users", "UserName", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Users", "Email", c => c.String(maxLength: 256));
            AlterColumn("dbo.StationGroups", "User_Id", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.Users", "Id");
            CreateIndex("dbo.Stations", "User_Id");
            CreateIndex("dbo.Users", "UserName", unique: true, name: "UserNameIndex");
            CreateIndex("dbo.StationGroups", "User_Id");
            AddForeignKey("dbo.Stations", "User_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.StationGroups", "User_Id", "dbo.Users", "Id");
            DropColumn("dbo.Users", "Password");
            DropColumn("dbo.Users", "ConfirmPassword");
            DropColumn("dbo.Users", "DateCreated");
            DropColumn("dbo.Users", "DateModified");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "DateModified", c => c.DateTime());
            AddColumn("dbo.Users", "DateCreated", c => c.DateTime());
            AddColumn("dbo.Users", "ConfirmPassword", c => c.String(maxLength: 100));
            AddColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 100));
            DropForeignKey("dbo.StationGroups", "User_Id", "dbo.Users");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.Users");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.StationGroups", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.Stations", new[] { "User_Id" });
            DropPrimaryKey("dbo.Users");
            AlterColumn("dbo.StationGroups", "User_Id", c => c.Int());
            AlterColumn("dbo.Users", "Email", c => c.String(maxLength: 100));
            AlterColumn("dbo.Users", "UserName", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Users", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Stations", "User_Id", c => c.Int(nullable: false));
            DropColumn("dbo.Users", "AccessFailedCount");
            DropColumn("dbo.Users", "LockoutEnabled");
            DropColumn("dbo.Users", "LockoutEndDateUtc");
            DropColumn("dbo.Users", "TwoFactorEnabled");
            DropColumn("dbo.Users", "PhoneNumberConfirmed");
            DropColumn("dbo.Users", "PhoneNumber");
            DropColumn("dbo.Users", "SecurityStamp");
            DropColumn("dbo.Users", "PasswordHash");
            DropColumn("dbo.Users", "EmailConfirmed");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            AddPrimaryKey("dbo.Users", "Id");
            CreateIndex("dbo.StationGroups", "User_Id");
            CreateIndex("dbo.Stations", "User_Id");
            AddForeignKey("dbo.StationGroups", "User_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.Stations", "User_Id", "dbo.Users", "Id");
        }
    }
}
