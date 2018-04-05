namespace PatientPhoto.Entity
{
    using System.Data.Entity;
using System.IO;

    public partial class AriaEntityContext : DbContext
    {
        public AriaEntityContext(string connectionString)
//            : base("name=AriaEntityContext")
            : base(connectionString)
        {
            Database.SetInitializer<AriaEntityContext>(null);
            Database.Log = s => File.AppendAllText(@"\\server\va_data$\katas\Advanced.X\PatientPhoto\log.txt", s);
        }

        public virtual DbSet<Doctor> Doctors { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .HasMany(e => e.Photos)
                .WithRequired(e => e.Patient)
                .WillCascadeOnDelete(false);
        }
    }
}
