namespace PatientPhoto.Entity2
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Doctor")]
    public partial class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ResourceSer { get; set; }

        [Required]
        [StringLength(16)]
        public string DoctorId { get; set; }

        [StringLength(16)]
        public string Honorific { get; set; }

        [Required]
        [StringLength(64)]
        public string FirstName { get; set; }

        [StringLength(64)]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(64)]
        public string LastName { get; set; }

        [StringLength(16)]
        public string NameSuffix { get; set; }

        [Required]
        [StringLength(64)]
        public string AliasName { get; set; }

        [StringLength(64)]
        public string Specialty { get; set; }

        [StringLength(64)]
        public string Institution { get; set; }

        public int OncologistFlag { get; set; }

        [StringLength(254)]
        public string Comment { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(16)]
        public string Sex { get; set; }
    }
}
