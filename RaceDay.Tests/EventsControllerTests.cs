using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Tests;

public class EventsControllerTests
{
    private static EventRequest NewEvent() => new()
    {
        Name = "Soweto 10km", Description = "Fun run", Date = DateTime.UtcNow.AddMonths(2),
        Location = "Soweto", DistanceKm = 10, EventType = EventType.Run
    };

    [Fact]
    public async Task Create_AsOrganiser_SavesEventAgainstTheOrganiser()
    {
        using var db = TestHelpers.NewDb();
        var org = db.AddUser("org@x.com", UserRole.Organiser);
        var controller = new EventsController(db).WithSession(org.Id, "Organiser");

        var result = await controller.Create(NewEvent());

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(org.Id, db.Events.Single().OrganiserId);
    }

    [Fact]
    public async Task Update_OtherOrganisersEvent_IsForbidden()
    {
        using var db = TestHelpers.NewDb();
        var owner = db.AddUser("owner@x.com", UserRole.Organiser);
        var other = db.AddUser("other@x.com", UserRole.Organiser);
        db.Events.Add(new Event { Name = "E", Description = "d", Location = "l", DistanceKm = 5, OrganiserId = owner.Id });
        db.SaveChanges();
        var controller = new EventsController(db).WithSession(other.Id, "Organiser");

        var result = await controller.Update(db.Events.Single().Id, NewEvent());

        Assert.Equal(403, ((ObjectResult)result).StatusCode);
        Assert.Equal("E", db.Events.Single().Name);   // unchanged
    }

    [Fact]
    public async Task Delete_OwnEvent_RemovesIt()
    {
        using var db = TestHelpers.NewDb();
        var owner = db.AddUser("owner@x.com", UserRole.Organiser);
        db.Events.Add(new Event { Name = "E", Description = "d", Location = "l", DistanceKm = 5, OrganiserId = owner.Id });
        db.SaveChanges();
        var controller = new EventsController(db).WithSession(owner.Id, "Organiser");

        var result = await controller.Delete(db.Events.Single().Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Events);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        using var db = TestHelpers.NewDb();
        var controller = new EventsController(db).WithSession(1, "Participant");

        Assert.IsType<NotFoundResult>(await controller.GetById(999));
    }
}
