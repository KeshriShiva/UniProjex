using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UniProjex.Api.Data;
using UniProjex.Api.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpClient();
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
builder.Services.AddDbContext<AppDbContext>(o => {
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        o.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
    else
        o.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => {
    o.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddPolicy("web", p => p.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"]).AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", service = "UniProjex API" }));
app.MapControllers();
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Projects.AnyAsync()) {
        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "projects.seed.json");
        if (File.Exists(seedPath)) {
            var json = await File.ReadAllTextAsync(seedPath);
            var rows = JsonSerializer.Deserialize<List<SeedProject>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            db.Projects.AddRange(rows.Select(x => new Project { Id=x.Id, UniversityName=x.UniversityName, Title=x.Title, Brief=x.Brief, ProjectLink=x.ProjectLink, Category=x.Category, Status=x.Status, StartDate=DateTime.Parse(x.StartDate) }));
            await db.SaveChangesAsync();
        }
    }
    var adminEmail = Environment.GetEnvironmentVariable("UNIPROJEX_ADMIN_EMAIL") ?? "admin@uniprojex.local";
    var adminPassword = Environment.GetEnvironmentVariable("UNIPROJEX_ADMIN_PASSWORD") ?? "ChangeMe!2026";
    if (!await db.Users.AnyAsync(x => x.Email == adminEmail.ToLower())) {
        db.Users.Add(new AppUser { FullName="UniProjex Administrator", Email=adminEmail.ToLower(), PasswordHash=BCrypt.Net.BCrypt.HashPassword(adminPassword), Role="Admin" });
        await db.SaveChangesAsync();
    }
}
app.Run();
record SeedProject(string Id,string UniversityName,string Title,string Brief,string ProjectLink,string Category,string Status,string StartDate);
