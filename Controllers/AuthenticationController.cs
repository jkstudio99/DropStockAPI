using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DropStockAPI.Extensions;
using DropStockAPI.Models;
using MailKit;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DropStockAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("EnableCors")]
    public class AuthenticationController : ControllerBase
    {
        // สร้าง Object ของ ApplicationDbContext
        private readonly ApplicationDbContext _context;

        // สร้าง Oject จัดการ Users
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        // ฟังก์ชันสร้าง Constructor สำหรับ initial ค่าของ ApplicationDbContext
        public AuthenticationController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            EmailService emailService
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        // ทดสอบเขียนฟังก์ชันการเชื่อมต่อ database
        [HttpGet("testconnectPostgreSQLdb")]
        public void TestConnection()
        {
            if (_context.Database.CanConnect())
            {
                // ถ้าเชื่อมต่อได้จะแสดงข้อความ "Connected"
                Response.WriteAsync("Connected");
            }
            else
            {
                // ถ้าเชื่อมต่อไม่ได้จะแสดงข้อความ "Not Connected"
                Response.WriteAsync("Not Connected");
            }
        }

        // Register for User
        // Post api/authenticate/register-user
        [HttpPost]
        [Route("register-user")]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterModel model)
        {
            // เช็คว่า username ซ้ำหรือไม่
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "User already exists!"
                    }
                );
            }

            // เช็คว่า email ซ้ำหรือไม่
            userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "Email already exists!"
                    }
                );
            }

            // สร้าง User
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            // สร้าง User ในระบบ
            var result = await _userManager.CreateAsync(user, model.Password);

            // ถ้าสร้างไม่สำเร็จ
            if (!result.Succeeded)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "User creation failed! Please check user details and try again."
                    }
                );
            }

            // ถ้าไม่มี Role Admin ให้สร้าง Role Admin ใหม่
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
            }

            // ถ้าไม่มี Role Manager ให้สร้าง Role Manager ใหม่
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.Manager))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Manager));
            }

            // ถ้าไม่มี Role User ให้สร้าง Role User ใหม่ และเพิ่ม User ลงใน Role User
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));
                await _userManager.AddToRoleAsync(user, UserRolesModel.User);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, UserRolesModel.User);
            }

            return Ok(new ResponseModel
            {
                Status = "Success",
                Message = "User registered successfully"
            });
        }

        // Register for Manager
        // Post api/authenticate/register-manager
        [HttpPost]
        [Route("register-manager")]
        public async Task<ActionResult> RegisterManager([FromBody] RegisterModel model)
        {
            // เช็คว่า username ซ้ำหรือไม่
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "User already exists!"
                    }
                );
            }

            // เช็คว่า email ซ้ำหรือไม่
            userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "Email already exists!"
                    }
                );
            }

            // สร้าง User
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            // สร้าง User ในระบบ
            var result = await _userManager.CreateAsync(user, model.Password);

            // ถ้าสร้างไม่สำเร็จ
            if (!result.Succeeded)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "User creation failed! Please check user details and try again."
                    }
                );
            }

            // ถ้าไม่มี Role Admin ให้สร้าง Role Admin ใหม่
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
            }

            // ถ้าไม่มี Role User ให้สร้าง Role User ใหม่ และเพิ่ม User ลงใน Role User
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));
            }

            // ถ้าไม่มี Role Manager ให้สร้าง Role Manager ใหม่
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.Manager))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Manager));
            }
            else
            {
                await _userManager.AddToRoleAsync(user, UserRolesModel.Manager);
            }

            return Ok(new ResponseModel
            {
                Status = "Success",
                Message = "User registered successfully"
            });
        }

        // Register for Admin
        // Post api/authenticate/register-admin
        [HttpPost]
        [Route("register-admin")]
        public async Task<ActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            // เช็คว่า username ซ้ำหรือไม่
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "User already exists!"
                    }
                );
            }

            // เช็คว่า email ซ้ำหรือไม่
            userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "Email already exists!"
                    }
                );
            }

            // สร้าง User
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            // สร้าง User ในระบบ
            var result = await _userManager.CreateAsync(user, model.Password);

            // ถ้าสร้างไม่สำเร็จ
            if (!result.Succeeded)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ResponseModel
                    {
                        Status = "Error",
                        Message = "User creation failed! Please check user details and try again."
                    }
                );
            }

            // ถ้าไม่มี Role User ให้สร้าง Role User ใหม่ และเพิ่ม User ลงใน Role User
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));
            }

            // ถ้าไม่มี Role Manager ให้สร้าง Role Manager ใหม่
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.Manager))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Manager));
            }

            // ถ้าไม่มี Role Admin ให้สร้าง Role Admin ใหม่
            if (!await _roleManager.RoleExistsAsync(UserRolesModel.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
            }
            else
            {
                await _userManager.AddToRoleAsync(user, UserRolesModel.Admin);
            }

            return Ok(new ResponseModel
            {
                Status = "Success",
                Message = "User registered successfully"
            });
        }

        // Login for User
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username!);

            // ถ้า login สำเร็จ
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    userData = new
                    {
                        userName = user.UserName,
                        email = user.Email,
                        roles = userRoles
                    }
                });
            }

            // ถ้า login ไม่สำเร็จ
            return Unauthorized();
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:ValidIssuer"],  // "DropStockAPI"
                audience: _configuration["JwtSettings:ValidAudience"],  // "DropStockWebApp"
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"])), // 200 minutes
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }


        // Logout
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    return Ok(new ResponseModel { Status = "Success", Message = "User logged out!" });
                }
            }
            return Ok();
        }

        // Refresh Token
        [HttpPost]
        [Route("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
        {
            var authHeader = Request.Headers["Authorization"];
            if (authHeader.ToString().StartsWith("Bearer"))
            {
                var token = authHeader.ToString().Substring(7);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecurityKey"]!);

                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var user = new
                    {
                        Name = jwtToken.Claims.First(x => x.Type == "unique_name").Value,
                        Role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value
                    };

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                    var newToken = GetToken(authClaims);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(newToken),
                        expiration = newToken.ValidTo
                    });
                }
                catch
                {
                    return Unauthorized();
                }
            }

            return Unauthorized();
        }


        public class RefreshTokenModel
        {
            public required string Token { get; set; }
        }

        // Validate Token Endpoint
        [HttpPost]
        [Route("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenValidationModel model)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]!);

            try
            {
                tokenHandler.ValidateToken(model.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // Ensure the token is not expired
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                var roles = jwtToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();

                return Ok(new
                {
                    Status = "Success",
                    UserName = userName,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    Message = "Token is not valid",
                    Details = ex.Message
                });
            }
        }

        public class TokenValidationModel
        {
            public required string Token { get; set; }
        }

        // Forgot Password
        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "User with this email does not exist."
                });
            }

            // Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Authentication", new { token, email = user.Email }, Request.Scheme);

            // Send email using the injected EmailService instance
            var subject = "Password Reset Request";
            var message = $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>.";
            await _emailService.SendEmailAsync(user.Email, subject, message);  // Use _emailService instance here

            return Ok(new ResponseModel
            {
                Status = "Success",
                Message = "Password reset link has been sent to your email."
            });
        }


        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "User with this email does not exist."
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "Error resetting password. Please ensure the reset token is valid."
                });
            }

            return Ok(new ResponseModel
            {
                Status = "Success",
                Message = "Password has been reset successfully."
            });
        }



        [HttpPost]
        [Route("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "User with this email does not exist."
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
            {
                return BadRequest(new ResponseModel
                {
                    Status = "Error",
                    Message = "Error confirming email."
                });
            }

            return Ok(new ResponseModel
            {
                Status = "Success",
                Message = "Email confirmed successfully."
            });
        }
    }
}