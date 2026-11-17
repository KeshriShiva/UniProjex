using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniProjex.Api.Data;
using UniProjex.Api.Models;

namespace UniProjex.Api.Controllers;
[ApiController,Route("api/chat")]
public class ChatController(AppDbContext db,IHttpClientFactory factory,IConfiguration config):ControllerBase {
 private const string Scope = "You are UniProjex Assistant, the help chatbot for UniProjex, a centralized inter-university project repository and management portal. Answer ONLY questions about this website, its purpose, project repository, project submission and review workflow, technology architecture, database, security, and features. If asked anything unrelated, politely say you can only help with UniProjex and its project. Do not invent facts. The platform uses ASP.NET Core 8 Web API and C#, Entity Framework Core with SQLite by default (can be configured for SQL Server), JWT authentication, BCrypt password hashing, and a React/JavaScript frontend. It provides registration/login, searchable project repository, project CRUD for authenticated users with ownership checks and admin privileges, filters, analytics, and this chatbot. Initial project records come from the supplied University Projects Database v2 spreadsheet. Keep answers concise and clear.";
 [HttpPost]
 public async Task<IActionResult> Ask(ChatRequest req) {
  if(string.IsNullOrWhiteSpace(req.Message)||req.Message.Length>1500)return BadRequest(new{message="Please enter a question (up to 1,500 characters)."});
  if (!IsInScope(req.Message)) return Ok(new { answer = "I’m designed to answer questions only about UniProjex, its university project repository, features, workflow, database and technology. Please ask me something related to this platform.", source = "Scope guard" });
  var apiKey=config["Gemini:ApiKey"]; 
  if(!string.IsNullOrWhiteSpace(apiKey)) {
   try {
    var stats=await db.Projects.GroupBy(x=>x.Status).Select(g=>$"{g.Key}: {g.Count()}").ToListAsync();
    var prompt=$"{Scope}\nCurrent repository facts: {await db.Projects.CountAsync()} records; {await db.Projects.Select(x=>x.UniversityName).Distinct().CountAsync()} universities. Status counts: {string.Join(", ",stats)}.\nUser question: {req.Message}";
    var client=factory.CreateClient();
    var response=await client.PostAsJsonAsync($"https://generativelanguage.googleapis.com/v1beta/models/{config["Gemini:Model"]}:generateContent?key={apiKey}",new{contents=new[]{new{parts=new[]{new{text=prompt}}}},generationConfig=new{temperature=0.2,maxOutputTokens=450}});
    if(response.IsSuccessStatusCode) {
     using var doc=JsonDocument.Parse(await response.Content.ReadAsStringAsync());
     var answer=doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
     return Ok(new{answer,source="Gemini"});
    }
   } catch { /* use deterministic project FAQ fallback */ }
  }
  return Ok(new{answer=Fallback(req.Message),source="Built-in UniProjex knowledge"});
 }
 private static bool IsInScope(string q) {
  var s=q.ToLowerInvariant();
  if (new[]{"hi","hello","hey","thanks","thank you","good morning","good evening"}.Any(x=>s.Trim()==x)) return true;
  return new[]{"uniprojex","project","university","portal","repository","database","excel","dataset","dashboard","analytics","login","register","account","authentication","jwt","password","api","backend","frontend","asp.net","c#","react","javascript","gemini","chatbot","category","status","submit","submission","approval","workflow","institution","student","search","filter","technology","tech stack","architecture","feature","module","record","project id","project brief"}.Any(s.Contains);
 }
 private static string Fallback(string q) {
  var s=q.ToLowerInvariant();
  if(s.Contains("what")||s.Contains("about")||s.Contains("purpose")||s.Contains("objective")) return "UniProjex is a centralized inter-university project repository. It helps students and institutions discover, organize, submit and track academic projects through one searchable platform.";
  if(s.Contains("how")&& (s.Contains("work")||s.Contains("made")||s.Contains("built"))) return "The platform uses a React and JavaScript frontend with an ASP.NET Core 8 Web API written in C#. Entity Framework Core manages the database. Users register or sign in, browse and filter projects, and authenticated users can submit and manage records subject to ownership rules.";
  if(s.Contains("technology")||s.Contains("tech stack")||s.Contains("language")||s.Contains("framework")) return "Core stack: C# and ASP.NET Core 8 for the backend, Entity Framework Core for data access, SQLite by default (SQL Server can be configured), JWT for authentication, BCrypt for password hashing, and React with JavaScript/CSS for the frontend. Gemini API can power the chatbot when an API key is configured.";
  if(s.Contains("database")||s.Contains("data")||s.Contains("excel")||s.Contains("record")) return "The supplied v2 workbook seeds 303 project records across 102 universities, 9 categories and 6 statuses. Each record includes project ID, university, title, brief, repository link, category, status and start date.";
  if(s.Contains("login")||s.Contains("register")||s.Contains("account")||s.Contains("security")) return "Registration creates a Student account. Login returns a signed JWT. Passwords are BCrypt-hashed, protected API operations require authentication, and users can edit or delete their own submissions; administrators can manage all records.";
  if(s.Contains("feature")||s.Contains("module")||s.Contains("page")) return "Main modules include a public landing page, account registration and login, project repository with search and filters, project submission and management, analytics dashboard, and the UniProjex scoped chatbot.";
  return "I can help with UniProjex: its purpose, modules, project records, workflow, technology stack, database, authentication and chatbot. Please ask a question related to this platform.";
 }
}
