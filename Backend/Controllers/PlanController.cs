using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMania.Models;
using SportMania.Repository.Interface;
using SportMania.Services;

namespace SportMania.Controllers;

[ApiController]
[Route("api/plans")]
public class PlanController (IPlanRepository _planRepository): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plan>>> GetAll(CancellationToken cancellationToken)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Plan>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null)
        {
            return NotFound();
        }

        return Ok(plan);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Plan>> Create([FromBody] Plan plan, CancellationToken cancellationToken)
    {
        await _planRepository.AddAsync(plan, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = plan.PlanId }, plan);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Plan plan, CancellationToken cancellationToken)
    {
        if (id != plan.PlanId)
        {
            return BadRequest(new { error = "Mismatched plan id." });
        }

        await _planRepository.UpdateAsync(plan, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _planRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("media")]
    [Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<string>> GetMedia()
    {
        return Ok(GetMediaPaths());
    }

    [HttpPost("media")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadMedia(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageExtensions.Contains(extension))
            return BadRequest(new { error = "Invalid file type." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "File too large. Maximum size is 5MB." });

        var mediaDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Media");
        if (!Directory.Exists(mediaDir))
            Directory.CreateDirectory(mediaDir);

        var newFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(mediaDir, newFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Ok(new { path = $"/Media/{newFileName}" });
    }

    private static readonly string[] ImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg" };

    private List<string> GetMediaPaths()
    {
        var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Media");
        if (!Directory.Exists(root)) return new List<string>();

        return Directory.EnumerateFiles(root)
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Select(f => "/Media/" + Path.GetFileName(f))
            .OrderBy(x => x)
            .ToList();
    }
}