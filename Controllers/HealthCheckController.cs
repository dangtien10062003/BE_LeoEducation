using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.Services;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IImageStorageService _imageStorage;

    public HealthCheckController(ApplicationDbContext db, IImageStorageService imageStorage)
    {
        _db = db;
        _imageStorage = imageStorage;
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

    [HttpGet("r2")]
    public async Task<IActionResult> GetR2(CancellationToken cancellationToken)
    {
        var data = await _imageStorage.CheckHealthAsync(cancellationToken);
        return Ok(new
        {
            success = true,
            message = "R2 health check",
            data
        });
    }
}
