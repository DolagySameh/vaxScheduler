using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace vaxScheduler.Data.Model
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("Vaccine")]
        public int VaccineId { get; set; }


        [ForeignKey("vaccinationCenter")]
        public int CenterId { get; set; }

        [StringLength(20)]
        public string DoseNumber { get; set; }

        public DateTime ReservationDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";


        public User Patient { get; set; }
        public VaccinationCenter vaccinationCenter { get; set; }
        public Vaccine Vaccine { get; set; }
    }
}
