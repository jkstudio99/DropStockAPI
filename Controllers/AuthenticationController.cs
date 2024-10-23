using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DropStockAPI.Extensions;
using DropStockAPI.Helpers;
using DropStockAPI.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DropStockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("CorsDropStock")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Context สำหรับการเชื่อมต่อกับฐานข้อมูล
        private readonly UserManager<IdentityUser> _userManager; // จัดการเกี่ยวกับ Users
        private readonly RoleManager<IdentityRole> _roleManager; // จัดการเกี่ยวกับ Roles
        private readonly IConfiguration _configuration; // สำหรับอ่านค่า configuration จาก appsettings
        private readonly EmailService _emailService; // Service สำหรับส่งอีเมล
        private readonly TokenHelper _tokenHelper; // Helper สำหรับจัดการ JWT Token

        public AuthenticationController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            EmailService emailService,
            TokenHelper tokenHelper)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _tokenHelper = tokenHelper;
        }

        // ฟังก์ชันทดสอบการเชื่อมต่อฐานข้อมูล
        [HttpGet("testconnectPostgreSQLdb")]
        public IActionResult TestConnection()
        {
            if (_context.Database.CanConnect()) // ตรวจสอบว่าฐานข้อมูลสามารถเชื่อมต่อได้หรือไม่
            {
                return Ok("Connected"); // ถ้าเชื่อมต่อได้ให้แสดงข้อความ "Connected"
            }
            return StatusCode(StatusCodes.Status500InternalServerError, "Not Connected"); // ถ้าเชื่อมต่อไม่ได้แสดงข้อความ "Not Connected"
        }

        // ฟังก์ชันสำหรับสร้าง role ถ้าไม่มีอยู่แล้ว
        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName)) // ตรวจสอบว่า role มีอยู่หรือไม่
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName)); // ถ้าไม่มีให้สร้าง role ใหม่
            }
        }

        // ฟังก์ชันภายในสำหรับการสมัครผู้ใช้ โดยใช้ role ที่กำหนด
        private async Task<ActionResult> RegisterUserInternal(RegisterModel model, string role)
        {
            if (await _userManager.FindByNameAsync(model.Username) != null) // เช็คว่า username ซ้ำหรือไม่
            {
                return Conflict(new ResponseModel { Status = "Error", Message = "Username already exists!" });
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null) // เช็คว่า email ซ้ำหรือไม่
            {
                return Conflict(new ResponseModel { Status = "Error", Message = "Email already exists!" });
            }

            // สร้าง User ใหม่
            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password); // สร้าง User
            if (!result.Succeeded) // ถ้าสร้าง User ไม่สำเร็จ
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel
                {
                    Status = "Error",
                    Message = "User creation failed! Please check details and try again."
                });
            }

            await EnsureRoleExists(role); // ตรวจสอบและสร้าง role ถ้าไม่มี
            await _userManager.AddToRoleAsync(user, role); // เพิ่ม User ไปยัง Role

            return Ok(new ResponseModel { Status = "Success", Message = "User registered successfully" }); // ส่งข้อความยืนยัน
        }

        // สมัคร User ทั่วไป
        [HttpPost("register-user")]
        public Task<ActionResult> RegisterUser([FromBody] RegisterModel model) => RegisterUserInternal(model, UserRolesModel.User); // ใช้ฟังก์ชันภายในในการสมัคร User

        // สมัคร Manager
        [HttpPost("register-manager")]
        public Task<ActionResult> RegisterManager([FromBody] RegisterModel model) => RegisterUserInternal(model, UserRolesModel.Manager); // ใช้ฟังก์ชันภายในในการสมัคร Manager

        // สมัคร Admin
        [HttpPost("register-admin")]
        public Task<ActionResult> RegisterAdmin([FromBody] RegisterModel model) => RegisterUserInternal(model, UserRolesModel.Admin); // ใช้ฟังก์ชันภายในในการสมัคร Admin

        // ฟังก์ชันสำหรับการเข้าสู่ระบบ
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username); // ค้นหา User โดยใช้ Username
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password)) // ถ้า User ไม่เจอ หรือ รหัสผ่านไม่ถูกต้อง
            {
                return Unauthorized(new { Message = "Invalid login attempt" }); // ส่ง Unauthorized
            }

            var userRoles = await _userManager.GetRolesAsync(user); // ดึง Role ของ User
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role))); // เพิ่ม Role ลงใน Claim
            var token = GenerateToken(authClaims); // สร้าง JWT Token

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token), // ส่ง Token กลับไป
                expiration = token.ValidTo,
                userData = new { userName = user.UserName, email = user.Email, roles = userRoles }
            });
        }

        private JwtSecurityToken GenerateToken(IEnumerable<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]!)); // ใช้ Secret Key ในการเข้ารหัส
                                                                                                                               // ดึงเวลา expiry จากการตั้งค่าใน appsettings
            var expiryInMinutes = int.Parse(_configuration["JwtSettings:ExpiryInMinutes"]);

            return new JwtSecurityToken(
                issuer: _configuration["JwtSettings:ValidIssuer"],
                audience: _configuration["JwtSettings:ValidAudience"],
                expires: DateTime.Now.AddMinutes(expiryInMinutes), // ใช้เวลา expiry จากการตั้งค่า
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256) // ใช้ HmacSha256 ในการลงนาม
            );
        }

        // ฟังก์ชันสำหรับออกจากระบบ
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name; // ดึงชื่อผู้ใช้จาก Identity
            if (userName == null) return Ok(new { Message = "User not logged in" }); // ถ้าไม่มีผู้ใช้เข้าสู่ระบบ

            var user = await _userManager.FindByNameAsync(userName); // ค้นหา User
            if (user == null) return NotFound(new { Message = "User not found" }); // ถ้าไม่เจอ User

            await _userManager.UpdateSecurityStampAsync(user); // อัพเดต Security Stamp ของ User เพื่อบังคับให้ออกจากระบบทุกที่
            return Ok(new { Message = "User logged out!" }); // ส่งข้อความยืนยันการออกจากระบบ
        }

        // ฟังก์ชันสำหรับ Refresh Token
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
        {
            try
            {
                var principal = _tokenHelper.GetPrincipalFromExpiredToken(model.Token); // ดึง Principal จาก Token ที่หมดอายุแล้ว
                if (principal == null) return Unauthorized(new { Message = "Invalid refresh token" }); // ถ้า Token ไม่ถูกต้อง

                var newAccessToken = _tokenHelper.GenerateAccessToken(principal.Claims); // สร้าง Access Token ใหม่
                var newRefreshToken = _tokenHelper.GenerateRefreshToken(); // สร้าง Refresh Token ใหม่

                return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken }); // ส่ง Token กลับไป
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Message = ex.Message }); // ส่ง Unauthorized พร้อมกับข้อความข้อผิดพลาด
            }
        }

        // ฟังก์ชันสำหรับตรวจสอบ Token ว่าถูกต้องหรือไม่
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenValidationModel model)
        {
            var tokenHandler = new JwtSecurityTokenHandler(); // ใช้ JwtSecurityTokenHandler ในการตรวจสอบ Token
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]!); // ดึง Secret Key

            try
            {
                tokenHandler.ValidateToken(model.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // ตรวจสอบ Signing Key
                    IssuerSigningKey = new SymmetricSecurityKey(key), // ใช้คีย์ในการตรวจสอบ
                    ValidateIssuer = false, // ไม่ตรวจสอบ Issuer
                    ValidateAudience = false, // ไม่ตรวจสอบ Audience
                    ValidateLifetime = true, // ตรวจสอบว่าหมดอายุหรือไม่
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken); // ตรวจสอบ Token

                var jwtToken = (JwtSecurityToken)validatedToken; // แปลง SecurityToken เป็น JwtSecurityToken
                var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value; // ดึง Username จาก Claim
                var roles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList(); // ดึง Role จาก Claim

                return Ok(new { Status = "Success", UserName = userName, Roles = roles }); // ส่งข้อมูลผู้ใช้กลับไป
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Status = "Error", Message = "Token is not valid", Details = ex.Message }); // ส่ง Unauthorized พร้อมข้อผิดพลาด
            }
        }

        // ฟังก์ชันสำหรับ Forgot Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email); // ค้นหา User จาก Email
            if (user == null) return BadRequest(new { Message = "User with this email does not exist." }); // ถ้าไม่เจอ User

            var token = await _userManager.GeneratePasswordResetTokenAsync(user); // สร้าง Token สำหรับ Reset Password
            var resetLink = Url.Action("ResetPassword", "Authentication", new { token, email = user.Email }, Request.Scheme); // สร้างลิงก์สำหรับ Reset Password

            await _emailService.SendEmailAsync(user.Email, "Password Reset Request", $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>."); // ส่งอีเมลไปยังผู้ใช้
            return Ok(new { Message = "Password reset link has been sent to your email." }); // ยืนยันการส่งอีเมล
        }

        // ฟังก์ชันสำหรับ Reset Password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email); // ค้นหา User จาก Email
            if (user == null) return BadRequest(new { Message = "User with this email does not exist." }); // ถ้าไม่เจอ User

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.ConfirmPassword); // Reset Password โดยใช้ Token และรหัสผ่านใหม่
            if (!result.Succeeded) return BadRequest(new { Message = "Error resetting password. Please ensure the reset token is valid." }); // ถ้ามีข้อผิดพลาดในการ Reset Password

            return Ok(new { Message = "Password has been reset successfully." }); // ยืนยันการ Reset Password สำเร็จ
        }

    }
}
