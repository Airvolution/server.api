using System.Diagnostics.Tracing;

namespace server_api
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using server_api.Models;

    public partial class AirUDBCOE : DbContext
    {
        
        public AirUDBCOE()
            : base("name=AirUDBCOE")
        {
        }
        
        public AirUDBCOE(string connectionString)
            : base(connectionString.Equals("")?"name=AirUDBCOE":connectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<DataPoint> DataPoints { get; set; }
        public virtual DbSet<StationGroup> DeviceGroups { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Daily> Dailies { get; set; }
        public virtual DbSet<FrequentlyAskedQuestion> FrequentlyAskedQuestions { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Keyword> EventKeywords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Parameter>()
                .HasMany(e => e.DataPoints)
                .WithRequired(e => e.Parameter)
                .HasForeignKey(e => new { e.Parameter_Name, e.Parameter_Unit })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Station>()
                .HasMany(e => e.DataPoints)
                .WithRequired(e => e.Station)
                .HasForeignKey(e => e.Station_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Station>()
                .HasMany(e => e.Dailies)
                .WithRequired(e => e.Station)
                .HasForeignKey(e => e.Station_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Stations)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.User_Id)
                .WillCascadeOnDelete(false);
        }
    }
}
