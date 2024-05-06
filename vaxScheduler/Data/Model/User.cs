using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace vaxScheduler.Data.Model
{
    public class User : IdentityUser<int>
    {
        public string Status { get; set; } = "pending";
        public string FirstName { get; set; }
        public string LastName {  get; set; }
    }

}
