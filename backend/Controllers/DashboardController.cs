using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniProjex.Api.Data;
namespace UniProjex.Api.Controllers;
[ApiController,Route("api/dashboard")]
public class DashboardController(AppDbContext db):ControllerBase {
 [HttpGet("summary")]
 public async Task<IActionResult> Summary() {
  var total=await db.Projects.CountAsync();
  var status=await db.Projects.GroupBy(x=>x.Status).Select(g=>new{label=g.Key,value=g.Count()}).OrderByDescending(x=>x.value).ToListAsync();
  var category=await db.Projects.GroupBy(x=>x.Category).Select(g=>new{label=g.Key,value=g.Count()}).OrderByDescending(x=>x.value).ToListAsync();
  var universities=await db.Projects.Select(x=>x.UniversityName).Distinct().CountAsync();
  var recent=await db.Projects.OrderByDescending(x=>x.StartDate).Take(6).Select(x=>new{x.Id,x.Title,x.UniversityName,x.Category,x.Status,x.StartDate}).ToListAsync();
  return Ok(new{totalProjects=total,universities,statuses=status,categories=category,recent});
 }
}
