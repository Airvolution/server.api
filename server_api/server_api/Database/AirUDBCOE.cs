using System.Diagnostics.Tracing;

namespace server_api
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using server_api.Models;
    using System.Data.Entity.Validation;
    using System.Diagnostics;

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
        public override int SaveChanges()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entity in entities)
            {
                if (entity.Entity is BaseEntity)
                {
                    if (entity.State == EntityState.Added)
                    {
                        ((BaseEntity)entity.Entity).DateCreated = DateTime.Now;
                    }
                    ((BaseEntity)entity.Entity).DateModified = DateTime.Now;
                }
                
            }
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                               validationError.ErrorMessage);
                    }
                }
                throw dbEx;
            }
            
        }
    }
}
