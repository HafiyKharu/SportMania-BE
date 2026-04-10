using Microsoft.AspNetCore.Mvc;
using SportMania.Models;
using SportMania.Repository.Interface;

namespace SportMania.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController(IPlanRepository _planRepository) : ControllerBase
{
    [HttpGet("plans")]
    public async Task<ActionResult<IEnumerable<Plan>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);

        return Ok(plans);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok" });
}
