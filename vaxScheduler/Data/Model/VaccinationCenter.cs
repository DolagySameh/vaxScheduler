using System.ComponentModel.DataAnnotations;

namespace vaxScheduler.Data.Model
{
    public class VaccinationCenter
    {
        [Key]
        public int CenterId { get; set; }

        [Required]
        [StringLength(100)]
        public string CenterName { get; set; }

        [StringLength(255)]
        public string Location { get; set; }
        public int ContactInfo {  get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public ICollection<Vaccine> Vaccines { get; set; }
    }
}
