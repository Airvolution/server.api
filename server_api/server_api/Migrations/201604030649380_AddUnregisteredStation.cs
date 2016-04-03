namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUnregisteredStation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UnregisteredStations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UnregisteredStations");
        }
    }
}
