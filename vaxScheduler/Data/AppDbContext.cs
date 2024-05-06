using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using vaxScheduler.Data.Model;

namespace vaxScheduler.Data
{
    //IdentityDbContext has functionality(tables of identity,....) related to authentication and authorization 
    //IdentityDbContext take table AppUser which have properity of user will enter and make its fuctionality
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
       
        // DbSet properties for your entities
        public DbSet<User> Users { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<VaccinationCenter> VaccinationCenters { get; set; }
        public DbSet<Certificate> Certificates { get; set; }


        internal User Find(bool v)
        {
            throw new NotImplementedException();
        }
    }
}
