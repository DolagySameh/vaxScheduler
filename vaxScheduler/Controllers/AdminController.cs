using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vaxScheduler.Data;
using vaxScheduler.Data.Model;
using vaxScheduler.models;


namespace vaxScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        public AdminController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration, RoleManager<IdentityRole<int>> roleManager)
        {
            _db = db;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        private readonly AppDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        /*                                   Hashing Password                               */
        private string HashPassword(string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        /*                                   Get VaccineCenter                             */


        [HttpGet("Get-VaccinationCenter")]

        public ActionResult<IEnumerable<VaccinationCenterDto>> GetAllVaccinationCenters()
        {
            var centers = _db.VaccinationCenters.ToList();
            var centerDTOs = centers.Select(c => new VaccinationCenterDto
            {
                CenterId = c.CenterId,
                CenterName = c.CenterName,
                Location = c.Location,
                ContactInfo = c.ContactInfo,
                email = c.email,
                password = HashPassword(c.password)
            }).ToList();

            return Ok(centerDTOs);
        }



        /*                                   Add-vaccinationCenter                           */
        [HttpPost("Add-vaccinationCenter")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VaccinationCenterDto>> AddVaccinationCenter([FromBody] VaccinationCenterDto centerDTO)
        {
            var createdCenter = new VaccinationCenter
            {
                CenterName = centerDTO.CenterName,
                Location = centerDTO.Location,
                email = centerDTO.email,
                ContactInfo = centerDTO.ContactInfo,
                password = HashPassword(centerDTO.password)
            };

            _db.VaccinationCenters.Add(createdCenter);
            _db.SaveChanges();
            var user = new User { UserName = centerDTO.CenterName, Email = centerDTO.email, FirstName = centerDTO.CenterName, LastName = centerDTO.CenterName };
            var result = await _userManager.CreateAsync(user, HashPassword(centerDTO.password));

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "VaccinationCenter");
            }
            else
            {
                return BadRequest(result.Errors);
            }

            centerDTO.CenterId = createdCenter.CenterId;
            return CreatedAtAction(nameof(GetAllVaccinationCenters), centerDTO);
        }


        /*                                    update VaccinationCenter                           */
        [Authorize(Roles = "Admin")]
        [HttpPut("update-VaccinationCenter/{centerId}")]
        public async Task<IActionResult> UpdateVaccinationCenter(int centerId, [FromBody] VaccinationCenterDto centerDTO)
        {
            var existingCenter = _db.VaccinationCenters.Find(centerId);
            if (existingCenter == null)
            {
                return NotFound();
            }
            existingCenter.CenterName = centerDTO.CenterName;
            existingCenter.Location = centerDTO.Location;
            existingCenter.ContactInfo = centerDTO.ContactInfo;
            var user = await _userManager.FindByEmailAsync(existingCenter.email);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            user.UserName = centerDTO.CenterName;
            user.Email = centerDTO.email;
            user.FirstName = centerDTO.CenterName;
            user.LastName = centerDTO.CenterName;
            existingCenter.email = centerDTO.email;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            _db.SaveChanges();

            return NoContent();
        }


        /*                                          delete VaccinationCenter                         */
        [HttpDelete("delete-vaccinationCenter/{centerId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteVaccinationCenter(int centerId)
        {
            var existingCenter = _db.VaccinationCenters.Find(centerId);
            if (existingCenter == null)
            {
                return NotFound();
            }
            var user = _userManager.Users.FirstOrDefault(u => u.Email == existingCenter.email);

            if (user != null)
            {
                _userManager.RemoveFromRoleAsync(user, "VaccinationCenter").Wait();

                _userManager.DeleteAsync(user).Wait();
            }
            _db.VaccinationCenters.Remove(existingCenter);
            _db.SaveChanges();

            return NoContent();
        }


        /*                                          Get Vaccines                        */
        [HttpGet("vaccination-centers/{centerId}/Get-vaccines")]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<VaccineDTO>> GetVaccinesForCenter(long centerId)
        {
            var vaccines = _db.Vaccines.Where(v => v.CenterId == centerId).ToList();
            var vaccineDTOs = vaccines.Select(v => new VaccineDTO
            {
                VaccineId = v.VaccineId,
                Name = v.VaccineName,
                Precautions = v.Precautions,
                TimeGapBetweenDoses = v.TimeGapBetweenDoses
            }).ToList();

            return Ok(vaccineDTOs);
        }

        /*                                          Add Vaccine                        */
        [HttpPost("vaccination-centers/{centerId}/add-vaccines")]
        [Authorize(Roles = "Admin")]
        public ActionResult<VaccineDTO> AddVaccineForCenter([FromRoute] int centerId, [FromBody] VaccineDTO vaccineDTO)
        {
            var center = _db.VaccinationCenters.Find(centerId);
            if (center == null)
            {
                return NotFound("Vaccination center not found");
            }

            var newVaccine = new Vaccine
            {
                VaccineName = vaccineDTO.Name,
                Precautions = vaccineDTO.Precautions,
                TimeGapBetweenDoses = vaccineDTO.TimeGapBetweenDoses,
                CenterId = centerId
            };

            _db.Vaccines.Add(newVaccine);
            _db.SaveChanges();

            vaccineDTO.VaccineId = newVaccine.VaccineId;
            return CreatedAtAction(nameof(GetVaccinesForCenter), new { centerId = centerId }, vaccineDTO);
        }

        /*                                          update Vaccine                       */
        [HttpPut("vaccination-centers/{centerId}/update-vaccines/{vaccineId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateVaccineForCenter([FromRoute] int centerId, [FromRoute] int vaccineId, [FromBody] VaccineDTO vaccineDTO)
        {
            var existingVaccine = _db.Vaccines.FirstOrDefault(v => v.VaccineId == vaccineId && v.CenterId == centerId);
            if (existingVaccine == null)
            {
                return NotFound("Vaccine not found for the given center");
            }

            existingVaccine.VaccineName = vaccineDTO.Name;
            existingVaccine.Precautions = vaccineDTO.Precautions;
            existingVaccine.TimeGapBetweenDoses = vaccineDTO.TimeGapBetweenDoses;

            _db.SaveChanges();

            return NoContent();
        }

        /*                                          delete Vaccine                        */
        [HttpDelete("vaccination-centers/{centerId}/delete-vaccines/{vaccineId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteVaccineForCenter(int centerId, int vaccineId)
        {
            var existingVaccine = _db.Vaccines.FirstOrDefault(v => v.VaccineId == vaccineId && v.CenterId == centerId);
            if (existingVaccine == null)
            {
                return NotFound("Vaccine not found for the given center");
            }

            _db.Vaccines.Remove(existingVaccine);
            _db.SaveChanges();

            return NoContent();
        }

        /*                                      Get pending-registeration                            */
        [HttpGet("pending-registrations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRegistrations()
        {
            var pendingUsers = await _userManager.Users.Where(u => u.Status == "Pending").ToListAsync();
            return Ok(pendingUsers.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email }));
        }

        /*                                      Accept Registered User                            */
        [HttpPut("approve-registration/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRegistration(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Status = "Accepted";
            await _db.SaveChangesAsync();

            return NoContent();
        }


        /*                                    Reject Registered User                            */
        [HttpPut("reject-registration/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRegistration(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return NoContent();
        }




        /*
                [HttpPost("AddRoles")]
                public async Task<IActionResult> CreateRole([FromBody] dtoAddRole model)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(model.name);
                    if (!roleExists)
                    {
                        var role = new IdentityRole<int>(model.name);
                        var result = await _roleManager.CreateAsync(role);
                        if (result.Succeeded)
                        {
                            return Ok($"Role '{model.name}' created successfully");
                        }
                        return BadRequest(result.Errors);
                    }
                    return BadRequest($"Role '{model.name}' already exists");
                }*/

        /*                                  Admin  Register only first time                              *//*
        private string HashPassword(string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        [HttpPost("admin-register")]
        public async Task<IActionResult> AdminRegister([FromBody] RegisterDto registerDto)
        {
           

            var user = new User
            {
                LastName = registerDto.LastName,
                FirstName = registerDto.FirstName,
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                
                // Additional properties for the user, if any
            };

            var result = await _userManager.CreateAsync(user, HashPassword(registerDto.Password));

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Add user to Admin role
            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok("Admin user created successfully");
        }*/




    }
}
