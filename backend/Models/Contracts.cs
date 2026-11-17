namespace UniProjex.Api.Models;
public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record ProjectUpsert(string UniversityName, string Title, string Brief, string ProjectLink, string Category, string Status, DateTime StartDate);
public record ChatRequest(string Message);
