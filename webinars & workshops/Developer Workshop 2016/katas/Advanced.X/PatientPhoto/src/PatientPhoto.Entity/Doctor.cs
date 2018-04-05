namespace PatientPhoto.Entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Doctor")]
    public partial class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ResourceSer { get; set; }

        [Required]
        [StringLength(16)]
        public string DoctorId { get; set; }

        [Required]
        [StringLength(64)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(64)]
        public string LastName { get; set; }
    }
}
