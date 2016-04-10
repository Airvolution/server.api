namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PasswordReset : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ResetPasswordCodes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ResetCode = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResetPasswordCodes", "User_Id", "dbo.Users");
            DropIndex("dbo.ResetPasswordCodes", new[] { "User_Id" });
            DropTable("dbo.ResetPasswordCodes");
        }
    }
}
