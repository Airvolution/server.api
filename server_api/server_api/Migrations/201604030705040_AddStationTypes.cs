namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStationTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stations", "Type", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stations", "Type");
        }
    }
}
