using System.ComponentModel.DataAnnotations;

namespace vaxScheduler.models
{
    public class LoginDto
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
