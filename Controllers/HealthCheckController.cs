using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public HealthCheckController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await _db.Database.CanConnectAsync();
            var users = await _db.Users.CountAsync();

            return Ok(new
            {
                success = true,
                message = "Server is running",
                data = new
                {
                    status = "OK",
                    database = "Connected",
                    users
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new { success = true, message = "Server is running", data = new { status = "OK", database = "Disconnected", error = ex.Message } });
        }
    }
}
