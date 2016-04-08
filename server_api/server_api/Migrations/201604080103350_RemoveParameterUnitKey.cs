namespace server_api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveParameterUnitKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Dailies", new[] { "MaxParameter_Name", "MaxParameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.Dailies", new[] { "MinParameter_Name", "MinParameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.DataPoints", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.UserPreferencesParameters", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.Stations", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropForeignKey("dbo.ParameterAdjustments", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters");
            DropIndex("dbo.Dailies", new[] { "MaxParameter_Name", "MaxParameter_Unit" });
            DropIndex("dbo.Dailies", new[] { "MinParameter_Name", "MinParameter_Unit" });
            DropIndex("dbo.DataPoints", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.Stations", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.ParameterAdjustments", new[] { "Parameter_Name", "Parameter_Unit" });
            DropIndex("dbo.UserPreferencesParameters", new[] { "Parameter_Name", "Parameter_Unit" });
            DropPrimaryKey("dbo.Parameters");
            DropPrimaryKey("dbo.DataPoints");
            DropPrimaryKey("dbo.ParameterAdjustments");
            DropPrimaryKey("dbo.UserPreferencesParameters");
            AlterColumn("dbo.Parameters", "Unit", c => c.String(maxLength: 30));
            AddPrimaryKey("dbo.Parameters", "Name");
            AddPrimaryKey("dbo.DataPoints", new[] { "Time", "Station_Id", "Parameter_Name" });
            AddPrimaryKey("dbo.ParameterAdjustments", new[] { "Station_Id", "Parameter_Name" });
            AddPrimaryKey("dbo.UserPreferencesParameters", new[] { "UserPreferences_Id", "Parameter_Name" });
            CreateIndex("dbo.Dailies", "MaxParameter_Name");
            CreateIndex("dbo.Dailies", "MinParameter_Name");
            CreateIndex("dbo.DataPoints", "Parameter_Name");
            CreateIndex("dbo.Stations", "Parameter_Name");
            CreateIndex("dbo.ParameterAdjustments", "Parameter_Name");
            CreateIndex("dbo.UserPreferencesParameters", "Parameter_Name");
            AddForeignKey("dbo.Dailies", "MaxParameter_Name", "dbo.Parameters", "Name");
            AddForeignKey("dbo.Dailies", "MinParameter_Name", "dbo.Parameters", "Name");
            AddForeignKey("dbo.DataPoints", "Parameter_Name", "dbo.Parameters", "Name");
            AddForeignKey("dbo.UserPreferencesParameters", "Parameter_Name", "dbo.Parameters", "Name", cascadeDelete: true);
            AddForeignKey("dbo.Stations", "Parameter_Name", "dbo.Parameters", "Name");
            AddForeignKey("dbo.ParameterAdjustments", "Parameter_Name", "dbo.Parameters", "Name", cascadeDelete: true);
            DropColumn("dbo.Dailies", "MaxParameter_Unit");
            DropColumn("dbo.Dailies", "MinParameter_Unit");
            DropColumn("dbo.DataPoints", "Parameter_Unit");
            DropColumn("dbo.Stations", "Parameter_Unit");
            DropColumn("dbo.ParameterAdjustments", "Parameter_Unit");
            DropColumn("dbo.UserPreferencesParameters", "Parameter_Unit");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserPreferencesParameters", "Parameter_Unit", c => c.String(nullable: false, maxLength: 30));
            AddColumn("dbo.ParameterAdjustments", "Parameter_Unit", c => c.String(nullable: false, maxLength: 30));
            AddColumn("dbo.Stations", "Parameter_Unit", c => c.String(maxLength: 30));
            AddColumn("dbo.DataPoints", "Parameter_Unit", c => c.String(nullable: false, maxLength: 30));
            AddColumn("dbo.Dailies", "MinParameter_Unit", c => c.String(maxLength: 30));
            AddColumn("dbo.Dailies", "MaxParameter_Unit", c => c.String(maxLength: 30));
            DropForeignKey("dbo.ParameterAdjustments", "Parameter_Name", "dbo.Parameters");
            DropForeignKey("dbo.Stations", "Parameter_Name", "dbo.Parameters");
            DropForeignKey("dbo.UserPreferencesParameters", "Parameter_Name", "dbo.Parameters");
            DropForeignKey("dbo.DataPoints", "Parameter_Name", "dbo.Parameters");
            DropForeignKey("dbo.Dailies", "MinParameter_Name", "dbo.Parameters");
            DropForeignKey("dbo.Dailies", "MaxParameter_Name", "dbo.Parameters");
            DropIndex("dbo.UserPreferencesParameters", new[] { "Parameter_Name" });
            DropIndex("dbo.ParameterAdjustments", new[] { "Parameter_Name" });
            DropIndex("dbo.Stations", new[] { "Parameter_Name" });
            DropIndex("dbo.DataPoints", new[] { "Parameter_Name" });
            DropIndex("dbo.Dailies", new[] { "MinParameter_Name" });
            DropIndex("dbo.Dailies", new[] { "MaxParameter_Name" });
            DropPrimaryKey("dbo.UserPreferencesParameters");
            DropPrimaryKey("dbo.ParameterAdjustments");
            DropPrimaryKey("dbo.DataPoints");
            DropPrimaryKey("dbo.Parameters");
            AlterColumn("dbo.Parameters", "Unit", c => c.String(nullable: false, maxLength: 30));
            AddPrimaryKey("dbo.UserPreferencesParameters", new[] { "UserPreferences_Id", "Parameter_Name", "Parameter_Unit" });
            AddPrimaryKey("dbo.ParameterAdjustments", new[] { "Station_Id", "Parameter_Name", "Parameter_Unit" });
            AddPrimaryKey("dbo.DataPoints", new[] { "Time", "Station_Id", "Parameter_Name", "Parameter_Unit" });
            AddPrimaryKey("dbo.Parameters", new[] { "Name", "Unit" });
            CreateIndex("dbo.UserPreferencesParameters", new[] { "Parameter_Name", "Parameter_Unit" });
            CreateIndex("dbo.ParameterAdjustments", new[] { "Parameter_Name", "Parameter_Unit" });
            CreateIndex("dbo.Stations", new[] { "Parameter_Name", "Parameter_Unit" });
            CreateIndex("dbo.DataPoints", new[] { "Parameter_Name", "Parameter_Unit" });
            CreateIndex("dbo.Dailies", new[] { "MinParameter_Name", "MinParameter_Unit" });
            CreateIndex("dbo.Dailies", new[] { "MaxParameter_Name", "MaxParameter_Unit" });
            AddForeignKey("dbo.ParameterAdjustments", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters", new[] { "Name", "Unit" }, cascadeDelete: true);
            AddForeignKey("dbo.Stations", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters", new[] { "Name", "Unit" });
            AddForeignKey("dbo.UserPreferencesParameters", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters", new[] { "Name", "Unit" }, cascadeDelete: true);
            AddForeignKey("dbo.DataPoints", new[] { "Parameter_Name", "Parameter_Unit" }, "dbo.Parameters", new[] { "Name", "Unit" });
            AddForeignKey("dbo.Dailies", new[] { "MinParameter_Name", "MinParameter_Unit" }, "dbo.Parameters", new[] { "Name", "Unit" });
            AddForeignKey("dbo.Dailies", new[] { "MaxParameter_Name", "MaxParameter_Unit" }, "dbo.Parameters", new[] { "Name", "Unit" });
        }
    }
}
