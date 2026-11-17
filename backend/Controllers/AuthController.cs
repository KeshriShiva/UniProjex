using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UniProjex.Api.Data;
using UniProjex.Api.Models;

namespace UniProjex.Api.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase {
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req) {
        if (string.IsNullOrWhiteSpace(req.FullName) || !req.Email.Contains('@') || req.Password.Length < 8)
            return BadRequest(new { message = "Enter a name, valid email and password with at least 8 characters." });
        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email)) return Conflict(new { message = "An account with this email already exists." });
        var user = new AppUser { FullName=req.FullName.Trim(), Email=email, PasswordHash=BCrypt.Net.BCrypt.HashPassword(req.Password), Role="Student" };
        db.Users.Add(user); await db.SaveChangesAsync();
        return Ok(CreateSession(user));
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req) {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == req.Email.Trim().ToLower());
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Unauthorized(new { message = "Email or password is incorrect." });
        return Ok(CreateSession(user));
    }
    private object CreateSession(AppUser user) {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()), new Claim(ClaimTypes.Name,user.FullName), new Claim(ClaimTypes.Email,user.Email), new Claim(ClaimTypes.Role,user.Role) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var token = new JwtSecurityToken(config["Jwt:Issuer"],config["Jwt:Audience"],claims,expires:DateTime.UtcNow.AddHours(10),signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));
        return new { token = new JwtSecurityTokenHandler().WriteToken(token), user = new { user.Id,user.FullName,user.Email,user.Role } };
    }
}
