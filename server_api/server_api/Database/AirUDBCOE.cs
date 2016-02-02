namespace server_api
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AirUDBCOE : DbContext
    {
        
        public AirUDBCOE()
            : base("name=AirUDBCOE")
        {
        }
        
        public AirUDBCOE(string connectionString)
            : base(connectionString)
        {
        }

        public virtual DbSet<DataPoint> DataPoints { get; set; }
        public virtual DbSet<StationGroup> DeviceGroups { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<StationState> DeviceStates { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Devices_States_and_Datapoints> Devices_States_and_Datapoints { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
