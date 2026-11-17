using System.ComponentModel.DataAnnotations;
namespace UniProjex.Api.Models;

public class AppUser {
    public int Id { get; set; }
    [MaxLength(120)] public string FullName { get; set; } = "";
    [MaxLength(180)] public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    [MaxLength(30)] public string Role { get; set; } = "Student";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Project {
    [Key, MaxLength(30)] public string Id { get; set; } = "";
    [Required, MaxLength(220)] public string UniversityName { get; set; } = "";
    [Required, MaxLength(240)] public string Title { get; set; } = "";
    public string Brief { get; set; } = "";
    [MaxLength(600)] public string ProjectLink { get; set; } = "";
    [MaxLength(80)] public string Category { get; set; } = "Other";
    [MaxLength(40)] public string Status { get; set; } = "Proposed";
    public DateTime StartDate { get; set; }
    public int? CreatedById { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
