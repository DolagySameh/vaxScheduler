using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vaxScheduler.Data.Model;
using vaxScheduler.Data;
using vaxScheduler.models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;

namespace vaxScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        public PatientsController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration)
        {
               _db = db;
               _configuration = configuration;
               _userManager = userManager;
        }
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        
        /*                               Get all VaccinationCenter with all vaccines                        */
        [HttpGet("GetAllVaccinationCenters")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<VaccinationCenterDto>>> GetVaccinationCenters()
        {
            var centers = await _db.VaccinationCenters
                .Select(c => new VaccinationCenterDto
                {
                    CenterId = c.CenterId,
                    CenterName = c.CenterName,
                    Location = c.Location,
                    ContactInfo = c.ContactInfo,
                })
                .ToListAsync();

            return Ok(centers);
        }

        /*                             Get all vaccines related to vaccination centers                        */
        [HttpGet("{centerId}/GetVaccinesOfCenter")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<VaccineDTO>>> GetVaccinesByCenter(int centerId)
        {
            var vaccines = await _db.Vaccines
                .Where(v => v.CenterId == centerId)
                .Select(v => new VaccineDTO
                {
                    VaccineId = v.VaccineId,
                    Name = v.VaccineName,
                    Precautions = v.Precautions,
                    TimeGapBetweenDoses = v.TimeGapBetweenDoses
                })
                .ToListAsync();

            return Ok(vaccines);
        }

        /*                            Reserve Vaccination (first and second Does)                                */
        [HttpPost("ReserveDose")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult> ReserveDose(ReservationDTO reservationDTO)
        {
            // Check if the patient has already reserved twice
            var existingReservationsCount = await _db.Reservations
            .Where(r => r.PatientId == reservationDTO.PatientId)
            .CountAsync();

            if (existingReservationsCount >= 2)
            {
                return BadRequest("You have already reserved twice. You cannot reserve more.");
            }

            // Check if the patient has already reserved this vaccine
            var existingReservation = await _db.Reservations
            .FirstOrDefaultAsync(r => r.PatientId == reservationDTO.PatientId &&
                                       r.VaccineId == reservationDTO.VaccineId &&
                                       r.DoseNumber == reservationDTO.DoseNumber);

            if (existingReservation != null)
            {
                return BadRequest("You have already reserved this vaccine for the specified dose.");
            }

            // Check if it's a second dose reservation and ensure the first dose is already taken
            if (reservationDTO.DoseNumber == "second-dose")
            {
                var firstDoseReservation = await _db.Reservations
                    .FirstOrDefaultAsync(r => r.PatientId == reservationDTO.PatientId &&
                                           r.VaccineId == reservationDTO.VaccineId &&
                                           r.DoseNumber == "first-dose");

                if (firstDoseReservation == null)
                {
                    return BadRequest("You need to reserve the first dose before reserving the second dose.");
                }

                // Check if the center has accepted the first dose reservation
                if (firstDoseReservation.Status != "Accepted")
                {
                    return BadRequest("You cannot reserve the second dose until the center accepts your first dose reservation.");
                }
            }
            // Convert DTO to Reservation entity
            var reservation = new Reservation
            {
                PatientId = reservationDTO.PatientId,
                VaccineId = reservationDTO.VaccineId,
                CenterId = reservationDTO.centerId,
                DoseNumber = reservationDTO.DoseNumber.ToLower(),
                ReservationDate = reservationDTO.ReservationDate,
            };
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            return Ok($"Dose reservation ({reservationDTO.DoseNumber}) successful. Waiting for center acceptance.");
        }

        /*                        cannot reserve Second does befor center accept first does                           */

        [HttpGet("GetPatientsWithFirstDose")]
        [Authorize(Roles = "VaccinationCenter")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatientsWithFirstDose()
        {
            var patientsWithFirstDose = await _db.Reservations
                .Where(r => r.DoseNumber.ToLower() == "first-dose")
                .Select(r => new ReservationDTO
                {
                    PatientId = r.PatientId,
                    VaccineId = r.VaccineId,
                    ReservationId = r.ReservationId,
                    ReservationDate = r.ReservationDate,
                })
                .ToListAsync();

            return Ok(patientsWithFirstDose);
        }



        /*                                Get Certification after Second-Does                                            */


        /*[HttpGet]
        [Route("api/certificate/image/{certificateId}")]
        public async Task<IActionResult> GetCertificateImage(int certificateId)
        {
            try
            {
                var certificate = await _db.Certificates.FindAsync(certificateId);

                if (certificate == null)
                {
                    return NotFound("Certificate not found.");
                }
                return File(certificate.CertificateFilePath, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
*/




    }
}
