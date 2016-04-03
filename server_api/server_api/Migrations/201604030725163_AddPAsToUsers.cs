namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPAsToUsers : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.ParameterAdjustments", "User_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.ParameterAdjustments", "User_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.ParameterAdjustments", "User_Id");
            AddForeignKey("dbo.ParameterAdjustments", "User_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParameterAdjustments", "User_Id", "dbo.Users");
            DropIndex("dbo.ParameterAdjustments", new[] { "User_Id" });
            DropColumn("dbo.ParameterAdjustments", "User_Id");
        }
    }
}
