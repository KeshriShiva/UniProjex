using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniProjex.Api.Data;
using UniProjex.Api.Models;

namespace UniProjex.Api.Controllers;
[ApiController, Route("api/projects")]
public class ProjectsController(AppDbContext db) : ControllerBase {
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery]string? q,[FromQuery]string? university,[FromQuery]string? category,[FromQuery]string? status,[FromQuery]int page=1,[FromQuery]int pageSize=12) {
        page=Math.Max(1,page); pageSize=Math.Clamp(pageSize,1,100);
        var query=db.Projects.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query=query.Where(x=>x.Title.Contains(q)||x.Brief.Contains(q)||x.UniversityName.Contains(q)||x.Id.Contains(q));
        if (!string.IsNullOrWhiteSpace(university)) query=query.Where(x=>x.UniversityName==university);
        if (!string.IsNullOrWhiteSpace(category)) query=query.Where(x=>x.Category==category);
        if (!string.IsNullOrWhiteSpace(status)) query=query.Where(x=>x.Status==status);
        var total=await query.CountAsync();
        var items=await query.OrderByDescending(x=>x.StartDate).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items,total,page,pageSize,totalPages=(int)Math.Ceiling(total/(double)pageSize) });
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(string id) { var p=await db.Projects.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id); return p is null?NotFound():Ok(p); }
    [Authorize, HttpPost]
    public async Task<IActionResult> Create(ProjectUpsert req) {
        if (!Valid(req)) return BadRequest(new {message="University, title and category are required."});
        var id="USR-"+Guid.NewGuid().ToString("N")[..10].ToUpper();
        var p=ToProject(id,req); p.CreatedById=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        db.Projects.Add(p); await db.SaveChangesAsync(); return CreatedAtAction(nameof(GetOne),new{id},p);
    }
    [Authorize, HttpPut("{id}")]
    public async Task<IActionResult> Update(string id,ProjectUpsert req) {
        var p=await db.Projects.FindAsync(id); if(p is null)return NotFound();
        if(!IsAdmin() && p.CreatedById!=CurrentUserId())return Forbid();
        if(!Valid(req))return BadRequest(new{message="University, title and category are required."});
        p.UniversityName=req.UniversityName.Trim();p.Title=req.Title.Trim();p.Brief=req.Brief??"";p.ProjectLink=req.ProjectLink??"";p.Category=req.Category;p.Status=req.Status;p.StartDate=req.StartDate;p.UpdatedAt=DateTime.UtcNow;
        await db.SaveChangesAsync();return Ok(p);
    }
    [Authorize, HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) {
        var p=await db.Projects.FindAsync(id);if(p is null)return NotFound();
        if(!IsAdmin() && p.CreatedById!=CurrentUserId())return Forbid();
        db.Projects.Remove(p);await db.SaveChangesAsync();return NoContent();
    }
    [HttpGet("meta/options")]
    public async Task<IActionResult> Options() => Ok(new {
        universities=await db.Projects.Select(x=>x.UniversityName).Distinct().OrderBy(x=>x).ToListAsync(),
        categories=await db.Projects.Select(x=>x.Category).Distinct().OrderBy(x=>x).ToListAsync(),
        statuses=await db.Projects.Select(x=>x.Status).Distinct().OrderBy(x=>x).ToListAsync()
    });
    private bool Valid(ProjectUpsert r)=>!string.IsNullOrWhiteSpace(r.UniversityName)&&!string.IsNullOrWhiteSpace(r.Title)&&!string.IsNullOrWhiteSpace(r.Category);
    private Project ToProject(string id,ProjectUpsert r)=>new(){Id=id,UniversityName=r.UniversityName.Trim(),Title=r.Title.Trim(),Brief=r.Brief??"",ProjectLink=r.ProjectLink??"",Category=r.Category,Status=r.Status,StartDate=r.StartDate,UpdatedAt=DateTime.UtcNow};
    private int CurrentUserId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin()=>User.IsInRole("Admin");
}
