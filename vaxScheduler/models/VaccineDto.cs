namespace vaxScheduler.models
{
    public class VaccineDTO
    {
        public int VaccineId { get; set; }
        public string Name { get; set; }
        public string Precautions { get; set; }
        public int TimeGapBetweenDoses { get; set; }
    }

}
