using AlumniManagementSystem.Common;
using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Department;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                Name         = d.Name,
                Description  = d.Description,
            })
            .ToListAsync();

        return Ok(ApiResponse<List<DepartmentDto>>.Ok(departments, "Departments retrieved."));
    }
}
