using vaxScheduler.Data.Model;

namespace vaxScheduler.models
{
    public class VaccinationCenterDto
    {
        public int CenterId { get; set; }
        public string CenterName { get; set; }
        public string Location { get; set; }
        public int ContactInfo { get; set; }
        public string email { get; set; }
        public string password { get; set; }

    }
}
