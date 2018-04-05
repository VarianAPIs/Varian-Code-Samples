namespace PatientPhoto.Entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Photo")]
    public partial class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PhotoSer { get; set; }

        public long PatientSer { get; set; }

        public byte[] Picture { get; set; }

        public virtual Patient Patient { get; set; }
    }
}
