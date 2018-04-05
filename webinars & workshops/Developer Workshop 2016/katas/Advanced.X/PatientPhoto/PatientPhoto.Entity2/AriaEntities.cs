namespace PatientPhoto.Entity2
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AriaEntities : DbContext
    {
        public AriaEntities()
            : base("name=AriaEntities")
        {
        }

        public virtual DbSet<Doctor> Doctors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doctor>()
                .Property(e => e.Sex)
                .IsFixedLength();
        }
    }
}
