using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[Route("api")]
[SessionAuthorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class EnrolmentsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public EnrolmentsController(AppDbContext db) => _db = db;

    /// <summary>Enter an event by choosing one of its categories (Participant only).</summary>
    /// <remarks>Records the link between you, the event and the category. You can enter each event once.</remarks>
    /// <response code="201">Enrolment recorded.</response>
    /// <response code="400">The category does not belong to that event.</response>
    /// <response code="403">Only Participants can enter events.</response>
    /// <response code="404">Event not found.</response>
    /// <response code="409">You are already enrolled in this event.</response>
    [HttpPost("enrolments")]
    [SessionAuthorize(Roles = "Participant")]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(EnrolRequest request)
    {
        if (!await _db.Events.AnyAsync(e => e.Id == request.EventId)) return NotFound();

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.EventId == request.EventId);
        if (category is null) return BadRequest(new { message = "That category does not belong to this event." });

        if (await _db.Enrolments.AnyAsync(e => e.EventId == request.EventId && e.ParticipantId == CurrentUserId))
            return Conflict(new { message = "You are already enrolled in this event." });

        var enrolment = new Enrolment { EventId = request.EventId, CategoryId = request.CategoryId, ParticipantId = CurrentUserId };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var created = await Query().FirstAsync(e => e.Id == enrolment.Id);
        return StatusCode(StatusCodes.Status201Created, ToResponse(created));
    }

    /// <summary>View your own enrolments (Participant only).</summary>
    /// <response code="200">Your enrolments.</response>
    /// <response code="403">Only Participants have enrolments.</response>
    [HttpGet("enrolments/mine")]
    [SessionAuthorize(Roles = "Participant")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Mine()
    {
        var list = await Query().Where(e => e.ParticipantId == CurrentUserId).ToListAsync();
        return Ok(list.Select(ToResponse).ToList());
    }

    /// <summary>View all enrolments for one of your events (Organiser only).</summary>
    /// <response code="200">Enrolments for the event.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("events/{eventId:int}/enrolments")]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only view enrolments for your own events.");

        var list = await Query().Where(e => e.EventId == eventId).ToListAsync();
        return Ok(list.Select(ToResponse).ToList());
    }

    private IQueryable<Enrolment> Query() =>
        _db.Enrolments.Include(e => e.Event).Include(e => e.Participant).Include(e => e.Category);

    private static EnrolmentResponse ToResponse(Enrolment e) =>
        new(e.Id, e.EventId, e.Event!.Name, e.ParticipantId,
            $"{e.Participant!.FirstName} {e.Participant.LastName}", e.CategoryId, e.Category!.Name, e.EnrolledAt);
}
