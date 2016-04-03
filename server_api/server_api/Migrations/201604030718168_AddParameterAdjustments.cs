namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddParameterAdjustments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ParameterAdjustments",
                c => new
                    {
                        Station_Id = c.String(nullable: false, maxLength: 32),
                        Parameter_Name = c.String(nullable: false, maxLength: 30),
                        Parameter_Unit = c.String(nullable: false, maxLength: 30),
                        ScaleFactor = c.Double(nullable: false),
                        ShiftFactor = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.Station_Id, t.Parameter_Name, t.Parameter_Unit })
                .ForeignKey("dbo.Parameters", t => new { t.Parameter_Name, t.Parameter_Unit }, cascadeDelete: true)
                .ForeignKey("dbo.Stations", t => t.Station_Id, cascadeDelete: true)
                .Index(t => t.Station_Id)
                .Index(t => new { t.Parameter_Name, t.Parameter_Unit });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParameterAdjustments", "Station_Id", "dbo.Stations");
            DropForeignKey("dbo.ParameterAdjustments", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropIndex("dbo.ParameterAdjustments", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.ParameterAdjustments", new[] { "Station_Id" });
            DropTable("dbo.ParameterAdjustments");
        }
    }
}
