namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteSerialNumber : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DataPoints", "SerialNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataPoints", "SerialNumber", c => c.Int(nullable: false, identity: true));
        }
    }
}
