using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vaxScheduler.Data.Model;
using vaxScheduler.Data;
using vaxScheduler.models;

namespace vaxScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Account : ControllerBase
    {

        public Account(AppDbContext db, UserManager<User> userManager, IConfiguration configuration, RoleManager<IdentityRole<int>> roleManager)
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

        /*                                   Patient_register                                               */
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new User
            {
                LastName = registerDto.LastName,
                FirstName = registerDto.FirstName,
                UserName = registerDto.UserName,
                Email = registerDto.Email,
            };
            var result = await _userManager.CreateAsync(user, HashPassword(registerDto.Password));

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await _userManager.AddToRoleAsync(user, "patient");
            return Ok("Patient user created successfully");
        }

        /*                                     Login                                             */
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, HashPassword(loginDto.password)))
            {
                return Unauthorized();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isVaccinationCenter = await _userManager.IsInRoleAsync(user, "VaccinationCenter");

            if (isAdmin)
            {
                var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Admin"),
        };

                var adminToken = GenerateToken(adminClaims);
                return Ok(new { Token = adminToken });
            }
            else if (isVaccinationCenter)
            {
                var centerClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "VaccinationCenter"),
        };

                var centerToken = GenerateToken(centerClaims);
                return Ok(new { Token = centerToken });
            }
            else
            {
                var patientClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Patient"),
        };
                if (user.Status != "Accepted")
                {
                    return Unauthorized("User is not approved.");
                }

                var patientToken = GenerateToken(patientClaims);
                return Ok(new { Token = patientToken });
            }
        }

        /*                                      generate Token                                     */
        private string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(60),
                signingCredentials: credentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        /*                                     logout                                               */
        [HttpPost("logout")]
        public IActionResult ALogout()
        {
            return Redirect("/login");
        }



    }
}
