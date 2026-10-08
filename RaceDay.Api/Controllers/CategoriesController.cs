using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[Route("api/events/{eventId:int}/categories")]
[SessionAuthorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class CategoriesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    /// <summary>List the age or distance categories for an event (both roles).</summary>
    /// <response code="200">Categories for the event.</response>
    /// <response code="404">No event with that id.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.Id == eventId)) return NotFound();

        var cats = await _db.Categories.Where(c => c.EventId == eventId).OrderBy(c => c.Name).ToListAsync();
        return Ok(cats.Select(ToResponse).ToList());
    }

    /// <summary>Define a category such as "Under 20", "Senior" or "10km" for one of your events (Organiser only).</summary>
    /// <response code="201">Category created.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">No event with that id.</response>
    [HttpPost]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only add categories to your own events.");

        var cat = new Category { EventId = eventId, Name = request.Name, MinAge = request.MinAge, MaxAge = request.MaxAge };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToResponse(cat));
    }

    /// <summary>Delete a category from one of your events (Organiser only).</summary>
    /// <response code="204">Category deleted.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event or category not found.</response>
    /// <response code="409">Participants are already enrolled in this category.</response>
    [HttpDelete("{categoryId:int}")]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int eventId, int categoryId)
    {
        var cat = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.EventId == eventId);
        if (cat is null) return NotFound();
        if (cat.Event!.OrganiserId != CurrentUserId) return Forbidden("You can only change your own events.");
        if (await _db.Enrolments.AnyAsync(e => e.CategoryId == categoryId))
            return Conflict(new { message = "Participants are already enrolled in this category." });

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static CategoryResponse ToResponse(Category c) => new(c.Id, c.EventId, c.Name, c.MinAge, c.MaxAge);
}
