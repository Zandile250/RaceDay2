using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[Route("api/events")]
[SessionAuthorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class EventsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public EventsController(AppDbContext db) => _db = db;

    /// <summary>List all events (both roles).</summary>
    /// <response code="200">All events, soonest first.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events.OrderBy(e => e.Date).ToListAsync();
        return Ok(events.Select(ToResponse).ToList());
    }

    /// <summary>Get one event by id (both roles).</summary>
    /// <response code="200">The event.</response>
    /// <response code="404">No event with that id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        return ev is null ? NotFound() : Ok(ToResponse(ev));
    }

    /// <summary>Create an event (Organiser only). Name, description, date, location, distance and type (Run, Walk, Cycle).</summary>
    /// <response code="201">Event created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Only Organisers can create events.</response>
    [HttpPost]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(EventRequest request)
    {
        var ev = new Event { OrganiserId = CurrentUserId };
        Apply(ev, request);
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ToResponse(ev));
    }

    /// <summary>Update one of your own events (Organiser only).</summary>
    /// <response code="200">Event updated.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">No event with that id.</response>
    [HttpPut("{id:int}")]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only change your own events.");

        Apply(ev, request);
        await _db.SaveChangesAsync();
        return Ok(ToResponse(ev));
    }

    /// <summary>Delete one of your own events (Organiser only). Its categories, enrolments and results are removed too.</summary>
    /// <response code="204">Event deleted.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">No event with that id.</response>
    [HttpDelete("{id:int}")]
    [SessionAuthorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only delete your own events.");

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static void Apply(Event ev, EventRequest r)
    {
        ev.Name = r.Name;
        ev.Description = r.Description;
        ev.Date = r.Date;
        ev.Location = r.Location;
        ev.DistanceKm = r.DistanceKm;
        ev.EventType = r.EventType;
    }

    private static EventResponse ToResponse(Event e) =>
        new(e.Id, e.Name, e.Description, e.Date, e.Location, e.DistanceKm, e.EventType, e.OrganiserId);
}
