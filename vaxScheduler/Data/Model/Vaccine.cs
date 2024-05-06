using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace vaxScheduler.Data.Model
{
    public class Vaccine
    {
        [Key]
        public int VaccineId { get; set; }

        [Required]
        [StringLength(100)]
        public string VaccineName { get; set; }

        [Column(TypeName = "text")]
        public string Precautions { get; set; }

        [ForeignKey(nameof(VaccinationCenter))]
        public int CenterId { get; set; }

        public VaccinationCenter VaccinationCenter { get; set; }
        public int TimeGapBetweenDoses { get; set; }
    }
}