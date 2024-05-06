namespace vaxScheduler.models
{
    public class ReservationDTO
    {
        public int ReservationId { get; set; }
        public int PatientId { get; set; }
        public int VaccineId { get; set; }
        public int centerId { get; set; }
        public string DoseNumber { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}
