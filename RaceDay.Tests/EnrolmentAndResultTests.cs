using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Tests;

public class EnrolmentAndResultTests
{
    private static (RaceDay.Api.Data.AppDbContext db, User org, User part, Event ev, Category cat) Seed()
    {
        var db = TestHelpers.NewDb();
        var org = db.AddUser("org@x.com", UserRole.Organiser);
        var part = db.AddUser("part@x.com", UserRole.Participant);
        var ev = new Event { Name = "Cape Cycle", Description = "d", Location = "CT", DistanceKm = 40, EventType = EventType.Cycle, OrganiserId = org.Id };
        db.Events.Add(ev);
        db.SaveChanges();
        var cat = new Category { EventId = ev.Id, Name = "Senior" };
        db.Categories.Add(cat);
        db.SaveChanges();
        return (db, org, part, ev, cat);
    }

    [Fact]
    public async Task Enrol_ValidCategory_RecordsParticipantEventAndCategory()
    {
        var (db, _, part, ev, cat) = Seed();
        var controller = new EnrolmentsController(db).WithSession(part.Id, "Participant");

        var result = await controller.Enrol(new EnrolRequest { EventId = ev.Id, CategoryId = cat.Id });

        Assert.Equal(201, ((ObjectResult)result).StatusCode);
        var e = db.Enrolments.Single();
        Assert.Equal((part.Id, ev.Id, cat.Id), (e.ParticipantId, e.EventId, e.CategoryId));
    }

    [Fact]
    public async Task Enrol_TwiceInSameEvent_ReturnsConflict()
    {
        var (db, _, part, ev, cat) = Seed();
        var controller = new EnrolmentsController(db).WithSession(part.Id, "Participant");
        await controller.Enrol(new EnrolRequest { EventId = ev.Id, CategoryId = cat.Id });

        var result = await controller.Enrol(new EnrolRequest { EventId = ev.Id, CategoryId = cat.Id });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Enrol_CategoryFromDifferentEvent_ReturnsBadRequest()
    {
        var (db, _, part, ev, _) = Seed();
        var controller = new EnrolmentsController(db).WithSession(part.Id, "Participant");

        var result = await controller.Enrol(new EnrolRequest { EventId = ev.Id, CategoryId = 12345 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CaptureResult_ForOwnEvent_SavesResult()
    {
        var (db, org, part, ev, cat) = Seed();
        var enrolment = new Enrolment { EventId = ev.Id, ParticipantId = part.Id, CategoryId = cat.Id };
        db.Enrolments.Add(enrolment);
        db.SaveChanges();
        var controller = new ResultsController(db).WithSession(org.Id, "Organiser");

        var result = await controller.Capture(new CaptureResultRequest
        { EnrolmentId = enrolment.Id, FinishTime = TimeSpan.FromMinutes(95), Position = 3 });

        Assert.Equal(201, ((ObjectResult)result).StatusCode);
        Assert.Equal(3, db.Results.Single().Position);
    }

    [Fact]
    public async Task CaptureResult_ForAnotherOrganisersEvent_IsForbidden()
    {
        var (db, _, part, ev, cat) = Seed();
        var otherOrg = db.AddUser("other@x.com", UserRole.Organiser);
        var enrolment = new Enrolment { EventId = ev.Id, ParticipantId = part.Id, CategoryId = cat.Id };
        db.Enrolments.Add(enrolment);
        db.SaveChanges();
        var controller = new ResultsController(db).WithSession(otherOrg.Id, "Organiser");

        var result = await controller.Capture(new CaptureResultRequest
        { EnrolmentId = enrolment.Id, FinishTime = TimeSpan.FromMinutes(95), Position = 1 });

        Assert.Equal(403, ((ObjectResult)result).StatusCode);
        Assert.Empty(db.Results);
    }

    [Fact]
    public async Task MyResults_ReturnsOnlyTheParticipantsOwnResults()
    {
        var (db, _, part, ev, cat) = Seed();
        var other = db.AddUser("p2@x.com", UserRole.Participant);
        var mine = new Enrolment { EventId = ev.Id, ParticipantId = part.Id, CategoryId = cat.Id };
        var theirs = new Enrolment { EventId = ev.Id, ParticipantId = other.Id, CategoryId = cat.Id };
        db.Enrolments.AddRange(mine, theirs);
        db.SaveChanges();
        db.Results.AddRange(
            new Result { EnrolmentId = mine.Id, FinishTime = TimeSpan.FromMinutes(60), Position = 1 },
            new Result { EnrolmentId = theirs.Id, FinishTime = TimeSpan.FromMinutes(70), Position = 2 });
        db.SaveChanges();
        var controller = new ResultsController(db).WithSession(part.Id, "Participant");

        var ok = Assert.IsType<OkObjectResult>(await controller.Mine());

        var list = Assert.IsType<List<ResultResponse>>(ok.Value);
        Assert.Single(list);
        Assert.Equal(1, list[0].Position);
    }
}
