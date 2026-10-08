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
public class ResultsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public ResultsController(AppDbContext db) => _db = db;

    /// <summary>Capture a participant's finish time and finishing position (Organiser only).</summary>
    /// <remarks>Send the <c>enrolmentId</c>, a <c>finishTime</c> such as "01:23:45" and the <c>position</c>.</remarks>
    /// <response code="201">Result captured.</response>
    /// <response code="403">Not an Organiser, or the enrolment is for another Organiser's event.</response>
    /// <response code="404">Enrolment not found.</response>
    /// <response code="409">A result already exists for this enrolment.</response>
    [HttpPost("results")]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Capture(CaptureResultRequest request)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == request.EnrolmentId);
        if (enrolment is null) return NotFound();
        if (enrolment.Event!.OrganiserId != CurrentUserId) return Forbidden("You can only capture results for your own events.");
        if (await _db.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
            return Conflict(new { message = "A result has already been captured for this enrolment." });

        var result = new Result { EnrolmentId = request.EnrolmentId, FinishTime = request.FinishTime, Position = request.Position };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var created = await Query().FirstAsync(r => r.Id == result.Id);
        return StatusCode(StatusCodes.Status201Created, ToResponse(created));
    }

    /// <summary>View your own results (Participant only).</summary>
    /// <response code="200">Your results.</response>
    /// <response code="403">Only Participants have personal results.</response>
    [HttpGet("results/mine")]
    [SessionAuthorize(Roles = "Participant")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Mine()
    {
        var list = await Query().Where(r => r.Enrolment!.ParticipantId == CurrentUserId).ToListAsync();
        return Ok(list.Select(ToResponse).ToList());
    }

    /// <summary>View all results for one of your events (Organiser only).</summary>
    /// <response code="200">Results ordered by position.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet("events/{eventId:int}/results")]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only view results for your own events.");

        var list = await Query().Where(r => r.Enrolment!.EventId == eventId).OrderBy(r => r.Position).ToListAsync();
        return Ok(list.Select(ToResponse).ToList());
    }

    private IQueryable<Result> Query() =>
        _db.Results.Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                   .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
                   .Include(r => r.Enrolment).ThenInclude(e => e!.Category);

    private static ResultResponse ToResponse(Result r) =>
        new(r.Id, r.EnrolmentId, r.Enrolment!.EventId, r.Enrolment.Event!.Name,
            $"{r.Enrolment.Participant!.FirstName} {r.Enrolment.Participant.LastName}",
            r.Enrolment.Category!.Name, r.FinishTime, r.Position);
}
