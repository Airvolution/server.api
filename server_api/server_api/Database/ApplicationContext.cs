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
    using Microsoft.AspNet.Identity.EntityFramework;

    public partial class ApplicationContext : IdentityDbContext<User>
    {

        public ApplicationContext()
            : base("name=AirDB")
        {
        }

        public ApplicationContext(string connectionString)
            : base(connectionString.Equals("") ? "name=AirDB" : connectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        //Identity and Authorization

        public virtual DbSet<DataPoint> DataPoints { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Station> Stations { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<Daily> Dailies { get; set; }
        public virtual DbSet<FrequentlyAskedQuestion> FrequentlyAskedQuestions { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Keyword> EventKeywords { get; set; }
        public virtual DbSet<UnregisteredStation> UnregisteredStations { get; set; }
        public virtual DbSet<ParameterAdjustment> ParameterAdjustments { get; set; }
        public virtual DbSet<UserPreferences> UserPreferences { get; set; }
        public virtual DbSet<ResetPasswordCode> ResetPasswordCodes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Parameter>()
                .HasMany(e => e.DataPoints)
                .WithRequired(e => e.Parameter)
                .HasForeignKey(e => new { e.Parameter_Name})
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
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPreferences>()
                .HasMany(e => e.DefaultParameters)
                .WithMany(e => e.UserPreferences)
                .Map(t => t.ToTable("UserPreferencesParameters")
                        .MapLeftKey("UserPreferences_Id")
                        .MapRightKey("Parameter_Name"));

            modelBuilder.Entity<Group>()
                .HasMany(e=>e.Stations)
                .WithMany(e=>e.Groups)
                .Map(t => t.ToTable("StationGroups")
                    .MapLeftKey("Group_Id")
                    .MapRightKey("Station_Id"));

            modelBuilder.Entity<FrequentlyAskedQuestion>()
                .HasMany(e => e.UserReviews)
                .WithRequired(e => e.FrequentlyAskedQuestion)
                .HasForeignKey(e => e.FrequentlyAskedQuestion_Id)
                .WillCascadeOnDelete(true);


            // Configure Asp Net Identity Tables
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(500);
            modelBuilder.Entity<User>().Property(u => u.PhoneNumber).HasMaxLength(50);
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